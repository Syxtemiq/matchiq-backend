namespace MatchIQ.Domain.Interfaces;

// Contrato para el servicio de email
// La implementación concreta usa MailKit en Infrastructure/Email/
public interface IEmailService
{
    Task SendVerificationCodeAsync(string to, string code);
    Task SendPasswordResetAsync(string to, string resetLink);
    Task SendTestInvitationAsync(string to, string offerTitle, int timeLimitMinutes);
}
