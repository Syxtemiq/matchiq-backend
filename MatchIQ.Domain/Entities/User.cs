using MatchIQ.Domain.Enums;

namespace MatchIQ.Domain.Entities;

// Representa un usuario del sistema (admin, candidato o empresa)
// Todo usuario tiene exactamente un perfil según su rol
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }   // NULL si se registró con Google
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public string? GoogleId { get; set; }       // NULL si se registró con email
    public string? PictureUrl { get; set; }     // foto de perfil de Google
    public DateTime CreatedAt { get; set; }

    public CandidateProfile? CandidateProfile { get; set; }
    public CompanyProfile? CompanyProfile { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<EmailVerification> EmailVerifications { get; set; } = new List<EmailVerification>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
}
