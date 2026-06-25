using MatchIQ.Domain.Entities;

namespace MatchIQ.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
