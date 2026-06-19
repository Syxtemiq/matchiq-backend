namespace MatchIQ.Infrastructure.Email;

// Implementación de IEmailService usando MailKit
// Ya lo conoces del proyecto ComplejoDeportivo
public class MailKitEmailService // : IEmailService
{
    // TODO: inyectar IConfiguration para SMTP settings

    // TODO: SendVerificationCodeAsync(string to, string code)
    //       email con código de 6 dígitos, expira en 10 minutos

    // TODO: SendPasswordResetAsync(string to, string resetLink)

    // TODO: SendTestInvitationAsync(string to, string offerTitle, int timeLimitMinutes)
    //       notifica al candidato que fue seleccionado para presentar el test
    //       incluye el tiempo límite disponible
}
