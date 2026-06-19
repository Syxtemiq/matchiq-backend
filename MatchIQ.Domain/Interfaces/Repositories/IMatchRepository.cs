namespace MatchIQ.Domain.Interfaces.Repositories;

// Contrato de acceso a datos para matches
// El método RunMatching llama la función SQL get_candidate_matches() via FromSqlRaw
public interface IMatchRepository
{
    // TODO: Task<IEnumerable<MatchResultDto>> RunMatchingAsync(int offerId)
    // TODO: Task<IEnumerable<Match>> GetByOfferAsync(int offerId)
    // TODO: Task UpdateStageAsync(int matchId, MatchStage stage)
}
