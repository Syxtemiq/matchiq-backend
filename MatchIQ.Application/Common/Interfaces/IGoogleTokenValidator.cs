using MatchIQ.Application.Modules.Auth.Dtos;

namespace MatchIQ.Application.Common.Interfaces;

public interface IGoogleTokenValidator
{
    // Valida el ID token con los servidores de Google y retorna la info del usuario.
    // Lanza InvalidOperationException si el token es inválido, expirado o no corresponde al ClientId configurado.
    Task<GoogleUserInfoDto> ValidateAsync(string idToken);
}
