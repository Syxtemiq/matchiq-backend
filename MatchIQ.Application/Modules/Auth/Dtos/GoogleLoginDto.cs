using System.ComponentModel.DataAnnotations;
using MatchIQ.Domain.Enums;

namespace MatchIQ.Application.Modules.Auth.Dtos;

public class GoogleLoginDto
{
    [Required]
    public string IdToken { get; set; } = string.Empty;

    // Solo se usa para usuarios nuevos; para usuarios existentes se ignora
    public UserRole Role { get; set; }
}
