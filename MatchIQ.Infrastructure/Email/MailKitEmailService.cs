using MatchIQ.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace MatchIQ.Infrastructure.Email;

public class MailKitEmailService : IEmailService
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _fromName;

    public MailKitEmailService(IConfiguration config)
    {
        _host = config["Email:SmtpHost"]!;
        _port = int.Parse(config["Email:SmtpPort"] ?? "587");
        _username = config["Email:Username"]!;
        _password = config["Email:Password"]!;
        _fromName = config["Email:FromName"] ?? "MatchIQ";
    }

    public async Task SendVerificationCodeAsync(string to, string code)
    {
        var subject = "Confirma tu registro en MatchIQ";
        var html = $"""
            <div style="font-family:sans-serif;max-width:480px;margin:auto">
              <h2 style="color:#1a1a2e">Bienvenido a MatchIQ</h2>
              <p>Ingresa el siguiente código para verificar tu email:</p>
              <div style="font-size:32px;font-weight:bold;letter-spacing:8px;
                          background:#f4f4f4;padding:16px 24px;border-radius:8px;
                          text-align:center;margin:24px 0">
                {code}
              </div>
              <p style="color:#666;font-size:13px">El código expira en 10 minutos.</p>
            </div>
            """;

        await SendAsync(to, subject, html);
    }

    public async Task SendPasswordResetAsync(string to, string resetLink)
    {
        var subject = "Restablece tu contraseña — MatchIQ";
        var html = $"""
            <div style="font-family:sans-serif;max-width:480px;margin:auto">
              <h2 style="color:#1a1a2e">Restablecer contraseña</h2>
              <p>Haz clic en el siguiente botón para crear una nueva contraseña.
                 El enlace es válido por 1 hora.</p>
              <a href="{resetLink}"
                 style="display:inline-block;background:#4f46e5;color:#fff;
                        padding:12px 28px;border-radius:6px;text-decoration:none;
                        font-weight:bold;margin:16px 0">
                Restablecer contraseña
              </a>
              <p style="color:#666;font-size:13px">
                Si no solicitaste esto, ignora este correo.
              </p>
            </div>
            """;

        await SendAsync(to, subject, html);
    }

    public async Task SendTestInvitationAsync(string to, string offerTitle, int timeLimitMinutes)
    {
        var subject = $"Tienes un test técnico — {offerTitle}";
        var html = $"""
            <div style="font-family:sans-serif;max-width:480px;margin:auto">
              <h2 style="color:#1a1a2e">Test técnico disponible</h2>
              <p>Has sido seleccionado para completar el test técnico de la oferta:</p>
              <p style="font-weight:bold;font-size:18px">{offerTitle}</p>
              <p>Tienes <strong>{timeLimitMinutes} minutos</strong> para completarlo
                 una vez que lo inicies.</p>
              <p>Ingresa a la plataforma para comenzar.</p>
            </div>
            """;

        await SendAsync(to, subject, html);
    }

    private async Task SendAsync(string to, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _username));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_username, _password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
