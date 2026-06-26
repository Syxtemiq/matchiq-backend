using MatchIQ.Domain.Entities;
using MatchIQ.Domain.Enums;

namespace MatchIQ.Application.Common.Interfaces.Repositories;

public interface IMatchRepository
{
    // get_candidate_matches(): solo candidatos nuevos sin match previo (matching incremental)
    Task<IEnumerable<Match>> RunMatchingAsync(int offerId);

    // get_full_offer_ranking(): TODOS los candidatos, hace UPSERT en matches (Reevaluar)
    Task ReevaluateAllAsync(int offerId);

    Task<IEnumerable<Match>> GetByOfferAsync(int offerId);
    Task UpdateStageAsync(int matchId, MatchStage stage);
}
