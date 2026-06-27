using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Domain.Enums;
using MatchIQ.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace MatchIQ.Infrastructure.Payments;

public class StripeService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<StripeService> _logger;
    private readonly string _webhookSecret;
    private readonly string _successUrl;
    private readonly string _cancelUrl;

    public StripeService(
        AppDbContext context,
        ILogger<StripeService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        StripeConfiguration.ApiKey = configuration["Stripe:PrivateKey"]
            ?? throw new InvalidOperationException("Stripe:PrivateKey no configurado.");
        _webhookSecret = configuration["Stripe:WebhookSecret"] ?? "";
        _successUrl = configuration["Stripe:SuccessUrl"] ?? configuration["App:FrontendUrl"] ?? "http://localhost:3000";
        _cancelUrl  = configuration["Stripe:CancelUrl"]  ?? configuration["App:FrontendUrl"] ?? "http://localhost:3000";
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

        // Idempotencia: si ya existe una sesión abierta, devolver su URL directamente
        if (!string.IsNullOrEmpty(payment.PaymentCheckoutId))
        {
            try
            {
                var existing = await new SessionService().GetAsync(payment.PaymentCheckoutId);
                if (existing.Status == "open")
                    return existing.Url;
            }
            catch { /* sesión expirada o inválida — crear una nueva */ }

            payment.PaymentCheckoutId = null;
        }

        var amountInCents = (long)(offer.PricingTier.PriceCop * 100);

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "cop",
                        UnitAmount = amountInCents,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"MatchIQ — {offer.Title}",
                            Description = $"Tier {offer.PricingTier.Name} — hasta {offer.PricingTier.MaxCandidates} candidatos"
                        }
                    },
                    Quantity = 1
                }
            ],
            Mode = "payment",
            SuccessUrl = $"{_successUrl.TrimEnd('/')}?offerId={offerId}&session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl  = $"{_cancelUrl.TrimEnd('/')}?offerId={offerId}&success=false",
            Metadata   = new Dictionary<string, string> { ["offer_id"] = offerId.ToString() }
        };

        var session = await new SessionService().CreateAsync(options);

        payment.PaymentCheckoutId = session.Id;
        await _context.SaveChangesAsync();

        return session.Url;
    }

    public async Task ProcessWebhookAsync(string rawBody, string? signature = null)
    {
        Event stripeEvent;

        if (!string.IsNullOrEmpty(_webhookSecret) && !string.IsNullOrEmpty(signature))
        {
            try
            {
                stripeEvent = EventUtility.ConstructEvent(rawBody, signature, _webhookSecret);
            }
            catch (StripeException ex)
            {
                throw new InvalidOperationException($"Firma del webhook de Stripe inválida: {ex.Message}");
            }
        }
        else
        {
            // Sin secreto configurado: procesar sin verificar firma (solo para desarrollo)
            stripeEvent = EventUtility.ParseEvent(rawBody);
        }

        if (stripeEvent.Type != EventTypes.CheckoutSessionCompleted) return;

        var session = stripeEvent.Data.Object as Session;
        if (session is null) return;

        var payment = await _context.Payments
            .Include(p => p.JobOffer)
            .FirstOrDefaultAsync(p => p.PaymentCheckoutId == session.Id);

        if (payment is null)
        {
            _logger.LogWarning("Webhook Stripe: no se encontró pago con SessionId={Id}", session.Id);
            return;
        }

        if (payment.Status == PaymentStatus.Succeeded) return;

        payment.Status = PaymentStatus.Succeeded;
        payment.PaymentTransactionId = session.PaymentIntentId;
        payment.PaidAt = DateTime.UtcNow;

        payment.JobOffer.Status   = OfferStatus.Open;
        payment.JobOffer.PaidAt   = DateTime.UtcNow;
        payment.JobOffer.ExpiresAt = DateTime.UtcNow.AddMonths(3);

        await _context.SaveChangesAsync();

        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT * FROM get_full_offer_ranking({payment.JobOffer.Id})");

        _logger.LogInformation("Oferta {OfferId} activada tras pago Stripe {SessionId}.",
            payment.JobOffer.Id, session.Id);
    }

    public async Task<bool> VerifyAndActivateAsync(string sessionId, int userId)
    {
        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        // Verificar directamente con Stripe que el pago fue aprobado
        Session session;
        try
        {
            session = await new SessionService().GetAsync(sessionId);
        }
        catch (StripeException ex)
        {
            throw new InvalidOperationException($"No se pudo verificar la sesión de pago: {ex.Message}");
        }

        if (session.PaymentStatus != "paid")
            return false;

        var payment = await _context.Payments
            .Include(p => p.JobOffer)
            .FirstOrDefaultAsync(p => p.PaymentCheckoutId == sessionId);

        if (payment is null)
            throw new KeyNotFoundException("No se encontró el registro de pago para esta sesión.");

        if (payment.JobOffer.CompanyId != company.Id)
            throw new UnauthorizedAccessException("No tienes acceso a esta oferta.");

        // Idempotencia: si ya fue procesado, no hacer nada
        if (payment.Status == PaymentStatus.Succeeded)
            return true;

        payment.Status = PaymentStatus.Succeeded;
        payment.PaymentTransactionId = session.PaymentIntentId;
        payment.PaidAt = DateTime.UtcNow;

        payment.JobOffer.Status    = OfferStatus.Open;
        payment.JobOffer.PaidAt    = DateTime.UtcNow;
        payment.JobOffer.ExpiresAt = DateTime.UtcNow.AddMonths(3);

        await _context.SaveChangesAsync();

        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT * FROM get_full_offer_ranking({payment.JobOffer.Id})");

        _logger.LogInformation("Oferta {OfferId} activada por verificación directa de sesión {SessionId}.",
            payment.JobOffer.Id, sessionId);

        return true;
    }
}
