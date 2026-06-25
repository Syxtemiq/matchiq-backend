using MatchIQ.Domain.Enums;

namespace MatchIQ.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int UserId { get; }
    UserRole Role { get; }
    bool IsAuthenticated { get; }
}
