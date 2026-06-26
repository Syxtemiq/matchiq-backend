using MatchIQ.Domain.Enums;

namespace MatchIQ.Application.Modules.Auth.Dtos;

public class GoogleLoginDto
{
    // ID token que Google devuelve al frontend tras el flujo OAuth
    public string IdToken { get; set; } = string.Empty;

    // Solo se usa para usuarios nuevos; para usuarios existentes se ignora
    public UserRole Role { get; set; }
}
