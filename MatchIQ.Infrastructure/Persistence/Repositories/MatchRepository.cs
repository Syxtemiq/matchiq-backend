using MatchIQ.Domain.Entities;
using MatchIQ.Domain.Enums;
using MatchIQ.Application.Common.Interfaces.Repositories;

namespace MatchIQ.Infrastructure.Persistence.Repositories;

// Implementación del repositorio de matches
// Usa FromSqlRaw para llamar la función SQL get_candidate_matches(offer_id)
public class MatchRepository : IMatchRepository
{
    // TODO: inyectar AppDbContext

    // TODO: RunMatchingAsync(int offerId)
    //       var results = _context.Database
    //           .SqlQueryRaw<MatchResultRaw>("SELECT * FROM get_candidate_matches({0})", offerId)
    //           .ToListAsync()

    // TODO: GetByOfferAsync → _context.Matches
    //                                  .Where(m => m.OfferId == offerId)
    //                                  .Include(m => m.CandidateProfile)
    //                                  .OrderByDescending(m => m.MatchPercentage)

    // TODO: UpdateStageAsync → busca el match, cambia Stage, SaveChangesAsync
    public async Task<IEnumerable<Match>> RunMatchingAsync(int offerId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Match>> GetByOfferAsync(int offerId)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateStageAsync(int matchId, MatchStage stage)
    {
        throw new NotImplementedException();
    }
}
