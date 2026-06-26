using MatchIQ.Application.Common.Interfaces.Repositories;
using MatchIQ.Domain.Entities;
using MatchIQ.Domain.Enums;
using MatchIQ.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatchIQ.Infrastructure.Persistence.Repositories;

public class MatchRepository : IMatchRepository
{
    private readonly AppDbContext _context;

    public MatchRepository(AppDbContext context)
    {
        _context = context;
    }

    // Llama a la función PL/pgSQL get_candidate_matches(offer_id) y sincroniza
    // los resultados en la tabla matches. La función devuelve candidate_id y
    // match_percentage para cada candidato elegible.
    public async Task<IEnumerable<Match>> RunMatchingAsync(int offerId)
    {
        var rawResults = await _context.Database
            .SqlQuery<MatchResultRaw>(
                $"SELECT candidate_id AS \"CandidateId\", match_percentage AS \"MatchPercentage\" FROM get_candidate_matches({offerId})")
            .ToListAsync();

        foreach (var raw in rawResults)
        {
            var existing = await _context.Matches
                .FirstOrDefaultAsync(m => m.OfferId == offerId && m.CandidateId == raw.CandidateId);

            if (existing is null)
            {
                _context.Matches.Add(new Match
                {
                    OfferId = offerId,
                    CandidateId = raw.CandidateId,
                    MatchPercentage = raw.MatchPercentage,
                    Stage = MatchStage.Matched,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else if (existing.Stage == MatchStage.Matched)
            {
                existing.MatchPercentage = raw.MatchPercentage;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return await GetByOfferAsync(offerId);
    }

    public async Task<IEnumerable<Match>> GetByOfferAsync(int offerId) =>
        await _context.Matches
            .Include(m => m.CandidateProfile)
                .ThenInclude(cp => cp.User)
            .Include(m => m.CandidateProfile)
                .ThenInclude(cp => cp.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
            .Where(m => m.OfferId == offerId)
            .OrderByDescending(m => m.AdjustedScore ?? m.MatchPercentage)
            .ToListAsync();

    public async Task ReevaluateAllAsync(int offerId)
    {
        // get_full_offer_ranking hace UPSERT de todos los candidatos activos en matches.
        // Actualiza match_percentage para existentes, inserta nuevos. Nunca toca stage.
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT candidate_id FROM get_full_offer_ranking({offerId})");
    }

    public async Task UpdateStageAsync(int matchId, MatchStage stage)
    {
        var match = await _context.Matches.FindAsync(matchId)
            ?? throw new KeyNotFoundException($"Match {matchId} no encontrado.");

        match.Stage = stage;
        match.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private sealed class MatchResultRaw
    {
        public int CandidateId { get; init; }
        public decimal MatchPercentage { get; init; }
    }
}
