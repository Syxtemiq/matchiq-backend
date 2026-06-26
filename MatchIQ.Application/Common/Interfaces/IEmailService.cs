namespace MatchIQ.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendVerificationCodeAsync(string to, string code);
    Task SendPasswordResetAsync(string to, string resetLink);
    Task SendTestInvitationAsync(string to, string offerTitle, int timeLimitMinutes, string loginUrl);
    Task SendCandidateSelectedAsync(string to, string candidateName, string offerTitle, string companyName, string frontendUrl);
    Task SendCandidateRejectedAsync(string to, string candidateName, string offerTitle, string frontendUrl);
}
