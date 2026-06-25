using MatchIQ.Application.Common.Interfaces;

namespace MatchIQ.Infrastructure.Email;

// Implementación de IEmailService usando MailKit
// Ya lo conoces del proyecto ComplejoDeportivo
public class MailKitEmailService : IEmailService
{

    public async Task SendVerificationCodeAsync(string to, string code)
    {
        throw new NotImplementedException();
    }

    public async Task SendPasswordResetAsync(string to, string resetLink)
    {
        throw new NotImplementedException();
    }

    public async Task SendTestInvitationAsync(string to, string offerTitle, int timeLimitMinutes)
    {
        throw new NotImplementedException();
    }
}
