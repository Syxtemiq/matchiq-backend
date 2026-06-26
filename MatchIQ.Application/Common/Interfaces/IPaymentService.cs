namespace MatchIQ.Application.Common.Interfaces;

public interface IPaymentService
{
    Task<string> CreatePaymentLinkAsync(int offerId, int userId);
    Task ProcessWebhookAsync(string rawBody);
}
