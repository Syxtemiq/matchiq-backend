using MatchIQ.Domain.Enums;

namespace MatchIQ.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Cedula { get; set; }
    public string? PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public bool EmailVerified { get; set; } = false;
    public string? GoogleId { get; set; }
    public string? PictureUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public CandidateProfile? CandidateProfile { get; set; }
    public CompanyProfile? CompanyProfile { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<EmailVerification> EmailVerifications { get; set; } = new List<EmailVerification>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
}
