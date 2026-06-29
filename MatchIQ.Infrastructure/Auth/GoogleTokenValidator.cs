using Google.Apis.Auth;
using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Auth.Dtos;
using Microsoft.Extensions.Configuration;

namespace MatchIQ.Infrastructure.Auth;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly string _clientId;

    public GoogleTokenValidator(IConfiguration configuration)
    {
        _clientId = configuration["Google:ClientId"]
            ?? throw new InvalidOperationException("Google:ClientId no está configurado en appsettings.");
    }

    public async Task<GoogleUserInfoDto> ValidateAsync(string idToken)
    {
        GoogleJsonWebSignature.Payload payload;

        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_clientId]
                });
        }
        catch (InvalidJwtException ex)
        {
            throw new InvalidOperationException($"Token de Google inválido: {ex.Message}");
        }

        return new GoogleUserInfoDto
        {
            GoogleId = payload.Subject,
            Email = payload.Email,
            Name = payload.Name,
            PictureUrl = payload.Picture
        };
    }
}
