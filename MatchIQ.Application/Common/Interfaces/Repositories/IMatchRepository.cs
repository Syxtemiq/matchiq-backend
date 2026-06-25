using MatchIQ.Domain.Entities;
using MatchIQ.Domain.Enums;

namespace MatchIQ.Application.Common.Interfaces.Repositories;

// RunMatchingAsync llama la función SQL get_candidate_matches() via FromSqlRaw
public interface IMatchRepository
{
    Task<IEnumerable<Match>> RunMatchingAsync(int offerId);
    Task<IEnumerable<Match>> GetByOfferAsync(int offerId);
    Task UpdateStageAsync(int matchId, MatchStage stage);
}
