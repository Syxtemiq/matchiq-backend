namespace MatchIQ.Application.Modules.Admin.Dtos;

public class UserSummaryDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Cedula { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ProfileName { get; set; }  // nombre de empresa para company, null para candidatos
}
