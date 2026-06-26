using System.ComponentModel.DataAnnotations;

namespace MatchIQ.Application.Modules.Auth.Dtos;

public class VerifyEmailDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "El código debe ser de 6 dígitos.")]
    public string Code { get; set; } = string.Empty;
}
