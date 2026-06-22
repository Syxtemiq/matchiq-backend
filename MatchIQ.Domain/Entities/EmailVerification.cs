namespace MatchIQ.Domain.Entities;

// Código de verificación de email: 6 dígitos, expira en 10 minutos
public class EmailVerification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool Used { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
