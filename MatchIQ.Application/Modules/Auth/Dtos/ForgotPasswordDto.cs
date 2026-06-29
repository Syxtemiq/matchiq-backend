using System.ComponentModel.DataAnnotations;

namespace MatchIQ.Application.Modules.Auth.Dtos;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
