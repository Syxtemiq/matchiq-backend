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

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync();

        try
        {
            await _paymentService.ProcessWebhookAsync(rawBody);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Firma"))
        {
            _logger.LogWarning("Webhook Wompi rechazado: {Reason}", ex.Message);
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando webhook de Wompi.");
        }

        return Ok(ApiResponse.Ok());
    }
}
