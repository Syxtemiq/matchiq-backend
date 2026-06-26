namespace MatchIQ.Application.Modules.Auth.Dtos;

public class GoogleUserInfoDto
{
    public string GoogleId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? PictureUrl { get; set; }
}
