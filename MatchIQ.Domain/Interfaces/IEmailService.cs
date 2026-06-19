namespace MatchIQ.Domain.Interfaces;

// Contrato para el servicio de email
// La implementación concreta usa MailKit en Infrastructure/Email/
public interface IEmailService
{
    // TODO: Task SendVerificationCodeAsync(string to, string code)
    // TODO: Task SendPasswordResetAsync(string to, string resetLink)
    // TODO: Task SendTestInvitationAsync(string to, string offerTitle, int timeLimitMinutes)
}
