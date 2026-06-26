using System.ComponentModel.DataAnnotations;

namespace MatchIQ.Application.Modules.Auth.Dtos;

public class GoogleLoginDto
{
    [Required]
    public string IdToken { get; set; } = string.Empty;

    // Solo se usa para usuarios nuevos; para usuarios existentes se ignora
    [RegularExpression(@"(?i)^(Candidate|Company)$",
        ErrorMessage = "El rol debe ser 'Candidate' o 'Company'.")]
    public string? Role { get; set; }
}
