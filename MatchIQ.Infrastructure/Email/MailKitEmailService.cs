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

    public async Task SendCandidateSelectedAsync(
        string to, string candidateName, string offerTitle, string companyName, string frontendUrl)
    {
        var subject = $"¡Felicitaciones, {candidateName.Split(' ')[0]}! Fuiste seleccionado — MatchIQ";
        var dashboardUrl = $"{frontendUrl}/dashboard";
        var html = $"""
            <div style="font-family:sans-serif;max-width:520px;margin:auto;color:#1a1a2e">
              <h2 style="color:#1a1a2e;margin-bottom:4px">¡Felicitaciones, {candidateName.Split(' ')[0]}!</h2>
              <p style="color:#555;margin-top:0">Tienes una actualización importante sobre tu proceso.</p>

              <p>Nos complace informarte que <strong>{companyName}</strong> ha revisado tu desempeño
                 y ha decidido seleccionarte como candidato para la posición:</p>

              <p style="font-weight:bold;font-size:18px;background:#f0f4ff;
                        border-left:4px solid #4f46e5;padding:12px 16px;
                        border-radius:4px;margin:20px 0">
                {offerTitle}
              </p>

              <p>Este es un reconocimiento a tu perfil, tu preparación y el trabajo que demostraste
                 durante el proceso. La empresa se pondrá en contacto contigo directamente para
                 coordinar los próximos pasos.</p>

              <p>Mientras tanto, puedes revisar tu perfil y el estado de tu proceso en la plataforma:</p>

              <a href="{dashboardUrl}"
                 style="display:inline-block;background:#4f46e5;color:#fff;
                        padding:13px 30px;border-radius:8px;text-decoration:none;
                        font-weight:bold;font-size:15px;margin:8px 0">
                Ver mi perfil en MatchIQ
              </a>

              <p style="color:#888;font-size:13px;margin-top:24px">
                Si el botón no funciona, copia este enlace en tu navegador:<br>
                <span style="color:#4f46e5">{dashboardUrl}</span>
              </p>

              <hr style="border:none;border-top:1px solid #eee;margin:28px 0">
              <p style="color:#aaa;font-size:12px;margin:0">
                MatchIQ — Conectando talento con oportunidades reales
              </p>
            </div>
            """;

        await SendAsync(to, subject, html);
    }

    public async Task SendCandidateRejectedAsync(
        string to, string candidateName, string offerTitle, string frontendUrl)
    {
        var subject = $"Actualización sobre tu candidatura — MatchIQ";
        var profileUrl = $"{frontendUrl}/profile";
        var firstName = candidateName.Split(' ')[0];
        var html = $"""
            <div style="font-family:sans-serif;max-width:520px;margin:auto;color:#1a1a2e">
              <h2 style="color:#1a1a2e;margin-bottom:4px">Hola, {firstName}</h2>
              <p style="color:#555;margin-top:0">Queremos mantenerte informado sobre tu proceso.</p>

              <p>Gracias por el tiempo y el esfuerzo que dedicaste al proceso de selección para:</p>

              <p style="font-weight:bold;font-size:17px;background:#f4f4f4;
                        border-left:4px solid #d1d5db;padding:12px 16px;
                        border-radius:4px;margin:20px 0;color:#374151">
                {offerTitle}
              </p>

              <p>En esta ocasión la empresa ha decidido avanzar con otro perfil. Sabemos que esta
                 noticia no es la que esperabas, y queremos que sepas que no refleja tu valor
                 como profesional — los procesos de selección dependen de muchos factores, y a
                 veces se trata simplemente de encontrar el ajuste más preciso para ese momento.</p>

              <p>Lo que sí puedes hacer ahora:</p>
              <ul style="color:#374151;line-height:1.8">
                <li>Mantén tu perfil actualizado con tus skills y proyectos más recientes.</li>
                <li>Continúa explorando otras ofertas que se ajusten a tu perfil.</li>
                <li>El sistema te notificará automáticamente cuando haya nuevas oportunidades.</li>
              </ul>

              <a href="{profileUrl}"
                 style="display:inline-block;background:#374151;color:#fff;
                        padding:13px 30px;border-radius:8px;text-decoration:none;
                        font-weight:bold;font-size:15px;margin:16px 0">
                Actualizar mi perfil
              </a>

              <p style="color:#888;font-size:13px;margin-top:24px">
                Si el botón no funciona, copia este enlace:<br>
                <span style="color:#4f46e5">{profileUrl}</span>
              </p>

              <hr style="border:none;border-top:1px solid #eee;margin:28px 0">
              <p style="color:#aaa;font-size:12px;margin:0">
                MatchIQ — Conectando talento con oportunidades reales
              </p>
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
