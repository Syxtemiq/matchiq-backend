namespace MatchIQ.Application.Common.Interfaces;

public interface IPaymentService
{
    Task<string?> CreatePaymentLinkAsync(int offerId, int userId);
    Task ProcessWebhookAsync(string rawBody, string? signature = null);
    Task<bool> VerifyAndActivateAsync(string sessionId, int userId);
}
