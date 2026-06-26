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

    public async Task SendTestInvitationAsync(string to, string offerTitle, int timeLimitMinutes, string loginUrl)
    {
        var subject = $"Fuiste seleccionado para un test técnico — {offerTitle}";
        var html = $"""
            <div style="font-family:sans-serif;max-width:520px;margin:auto;color:#1a1a2e">
              <h2 style="color:#1a1a2e">¡Felicitaciones! Fuiste seleccionado</h2>
              <p>Una empresa revisó tu perfil en <strong>MatchIQ</strong> y te seleccionó
                 para presentar el test técnico de la siguiente oferta:</p>
              <p style="font-weight:bold;font-size:18px;background:#f4f4f4;
                        padding:12px 16px;border-radius:6px;margin:16px 0">
                {offerTitle}
              </p>
              <p>⏱ Tendrás <strong>{timeLimitMinutes} minutos</strong> para completarlo
                 una vez que lo inicies, así que asegúrate de tener tiempo disponible
                 antes de comenzar.</p>
              <p>Haz clic en el botón para ingresar a la plataforma e iniciar tu test:</p>
              <a href="{loginUrl}"
                 style="display:inline-block;background:#4f46e5;color:#fff;
                        padding:14px 32px;border-radius:8px;text-decoration:none;
                        font-weight:bold;font-size:16px;margin:16px 0">
                Ir a mi test
              </a>
              <p style="color:#888;font-size:13px;margin-top:24px">
                Si no puedes hacer clic en el botón, copia este enlace en tu navegador:<br>
                <span style="color:#4f46e5">{loginUrl}</span>
              </p>
              <hr style="border:none;border-top:1px solid #eee;margin:24px 0">
              <p style="color:#aaa;font-size:12px">MatchIQ — Plataforma de matching para desarrolladores</p>
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
