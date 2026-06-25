namespace MatchIQ.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendVerificationCodeAsync(string to, string code);
    Task SendPasswordResetAsync(string to, string resetLink);
    Task SendTestInvitationAsync(string to, string offerTitle, int timeLimitMinutes);
}
