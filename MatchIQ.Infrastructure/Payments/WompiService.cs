using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Domain.Enums;
using MatchIQ.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MatchIQ.Infrastructure.Payments;

public class WompiService : IPaymentService
{
    private readonly HttpClient _http;
    private readonly AppDbContext _context;
    private readonly ILogger<WompiService> _logger;
    private readonly string _privateKey;
    private readonly string _eventsSecret;
    private readonly string _baseUrl;
    private readonly string _redirectUrl;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public WompiService(
        HttpClient http,
        AppDbContext context,
        ILogger<WompiService> logger,
        IConfiguration configuration)
    {
        _http = http;
        _context = context;
        _logger = logger;
        _privateKey  = configuration["Wompi:PrivateKey"]
            ?? throw new InvalidOperationException("Wompi:PrivateKey no configurado.");
        _eventsSecret = configuration["Wompi:EventsSecret"]
            ?? throw new InvalidOperationException("Wompi:EventsSecret no configurado.");
        _baseUrl     = configuration["Wompi:BaseUrl"] ?? "https://sandbox.wompi.co/v1";
        _redirectUrl = configuration["Wompi:RedirectUrl"] ?? "";
    }

    public async Task<string> CreatePaymentLinkAsync(int offerId, int userId)
    {
        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        var offer = await _context.JobOffers
            .Include(o => o.PricingTier)
            .FirstOrDefaultAsync(o => o.Id == offerId && o.CompanyId == company.Id)
            ?? throw new KeyNotFoundException("Oferta no encontrada.");

        if (offer.Status != OfferStatus.PendingPayment)
            throw new InvalidOperationException("La oferta no está pendiente de pago.");

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.OfferId == offerId)
            ?? throw new KeyNotFoundException("Registro de pago no encontrado.");

        // Idempotencia: si ya se creó un link, devolver el mismo sin llamar a Wompi de nuevo
        if (!string.IsNullOrEmpty(payment.PaymentCheckoutId))
            return $"https://checkout.wompi.co/l/{payment.PaymentCheckoutId}";

        var amountInCents = (long)(offer.PricingTier.PriceCop * 100);

        var body = new
        {
            name = $"MatchIQ — {offer.Title}",
            description = $"Tier {offer.PricingTier.Name} — hasta {offer.PricingTier.MaxCandidates} candidatos",
            single_use = true,
            collect_shipping = false,
            currency = "COP",
            amount_in_cents = amountInCents,
            redirect_url = $"{_redirectUrl.TrimEnd('/')}?offerId={offerId}"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/payment_links")
        {
            Content = JsonContent.Create(body, options: _jsonOptions)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _privateKey);

        var response = await _http.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Error al crear link de pago en Wompi: {responseContent}");

        var wompiResponse = JsonSerializer.Deserialize<WompiLinkResponse>(responseContent, _jsonOptions)
            ?? throw new InvalidOperationException("Wompi no devolvió respuesta válida.");

        payment.PaymentCheckoutId = wompiResponse.Data.Id;
        await _context.SaveChangesAsync();

        return $"https://checkout.wompi.co/l/{wompiResponse.Data.Id}";
    }

    public Task<bool> VerifyAndActivateAsync(string sessionId, int userId) =>
        throw new NotSupportedException("Wompi no está en uso.");

    public async Task ProcessWebhookAsync(string rawBody, string? signature = null)
    {
        WompiWebhookPayload payload;
        try
        {
            payload = JsonSerializer.Deserialize<WompiWebhookPayload>(rawBody, _jsonOptions)
                ?? throw new InvalidOperationException("Payload vacío.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Webhook con formato inválido: {ex.Message}");
        }

        VerifySignature(payload);

        if (payload.Event != "transaction.updated") return;

        var transaction = payload.Data?.Transaction;
        if (transaction is null || transaction.Status != "APPROVED") return;

        var payment = await _context.Payments
            .Include(p => p.JobOffer)
            .FirstOrDefaultAsync(p => p.PaymentCheckoutId == transaction.PaymentLinkId);

        if (payment is null)
        {
            _logger.LogWarning("Webhook Wompi: no se encontró pago con PaymentCheckoutId={Id}", transaction.PaymentLinkId);
            return;
        }

        // Idempotencia: si ya fue procesado, ignorar
        if (payment.Status == PaymentStatus.Succeeded) return;

        payment.Status = PaymentStatus.Succeeded;
        payment.PaymentTransactionId = transaction.Id;
        payment.PaidAt = DateTime.UtcNow;

        payment.JobOffer.Status = OfferStatus.Open;
        payment.JobOffer.PaidAt = DateTime.UtcNow;
        payment.JobOffer.ExpiresAt = DateTime.UtcNow.AddMonths(3);

        await _context.SaveChangesAsync();

        // Matching inicial: evalúa todos los candidatos existentes contra la oferta recién activada
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT * FROM get_full_offer_ranking({payment.JobOffer.Id})");

        _logger.LogInformation("Oferta {OfferId} activada tras pago Wompi {TransactionId}.",
            payment.JobOffer.Id, transaction.Id);
    }

    private void VerifySignature(WompiWebhookPayload payload)
    {
        var sig = payload.Signature;
        var transaction = payload.Data?.Transaction;

        if (sig?.Properties is null || sig.Checksum is null || transaction is null)
            throw new InvalidOperationException("Firma del webhook de Wompi inválida o incompleta.");

        var transactionValues = new Dictionary<string, string>
        {
            ["transaction.id"]               = transaction.Id ?? "",
            ["transaction.status"]           = transaction.Status ?? "",
            ["transaction.amount_in_cents"]  = transaction.AmountInCents.ToString(),
            ["transaction.currency"]         = transaction.Currency ?? "",
            ["transaction.payment_link_id"]  = transaction.PaymentLinkId ?? ""
        };

        var concatenated = string.Concat(
            sig.Properties.Select(p => transactionValues.TryGetValue(p, out var v) ? v : "")
        );

        var toHash = $"{concatenated}{payload.Timestamp}{_eventsSecret}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(toHash));
        var computed = Convert.ToHexString(hashBytes).ToLower();

        if (computed != sig.Checksum)
            throw new InvalidOperationException("Firma del webhook de Wompi no coincide.");
    }

    // ── DTOs internos de Wompi ────────────────────────────────────────────────

    private sealed class WompiLinkResponse
    {
        public WompiLinkData Data { get; set; } = null!;
    }

    private sealed class WompiLinkData
    {
        public string Id { get; set; } = string.Empty;
    }

    private sealed class WompiWebhookPayload
    {
        public string Event { get; set; } = string.Empty;
        public WompiWebhookData? Data { get; set; }
        public long Timestamp { get; set; }
        public WompiSignature? Signature { get; set; }
    }

    private sealed class WompiWebhookData
    {
        public WompiTransaction? Transaction { get; set; }
    }

    private sealed class WompiTransaction
    {
        public string? Id { get; set; }
        public string? Status { get; set; }
        public long AmountInCents { get; set; }
        public string? Currency { get; set; }
        public string? PaymentLinkId { get; set; }
    }

    private sealed class WompiSignature
    {
        public List<string>? Properties { get; set; }
        public string? Checksum { get; set; }
    }
}
