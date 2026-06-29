using MatchIQ.API.Common;
using MatchIQ.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MatchIQ.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentService paymentService,
        ICurrentUserService currentUser,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _currentUser = currentUser;
        _logger = logger;
    }

    [EnableRateLimiting("payment")]
    [Authorize(Roles = "Company")]
    [HttpPost("create-checkout")]
    public async Task<IActionResult> CreateCheckout([FromQuery] int offerId)
    {
        var url = await _paymentService.CreatePaymentLinkAsync(offerId, _currentUser.UserId);
        return Ok(ApiResponse.Ok(new { url }));
    }

    [Authorize(Roles = "Company")]
    [HttpPost("verify-session")]
    public async Task<IActionResult> VerifySession([FromQuery] string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return BadRequest(ApiResponse.Fail("sessionId es requerido."));

        var activated = await _paymentService.VerifyAndActivateAsync(sessionId, _currentUser.UserId);
        return Ok(ApiResponse.Ok(new { activated }, activated
            ? "Pago verificado. Oferta activada."
            : "El pago aún no ha sido procesado."));
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();

        try
        {
            await _paymentService.ProcessWebhookAsync(rawBody, signature);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Firma"))
        {
            _logger.LogWarning("Webhook rechazado por firma inválida: {Reason}", ex.Message);
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando webhook de pago.");
        }

        return Ok(ApiResponse.Ok());
    }
}
