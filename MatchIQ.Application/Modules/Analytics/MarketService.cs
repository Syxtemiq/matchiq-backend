using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Analytics.Dtos;
using Microsoft.EntityFrameworkCore;

namespace MatchIQ.Application.Modules.Analytics;

public class MarketService
{
    private readonly IAppDbContext _context;

    public MarketService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<MarketSkillsDto> GetMarketAsync()
    {
        var (demand, supply, combos) = await FetchMarketDataAsync();
        return new MarketSkillsDto
        {
            TopDemand = demand,
            TopSupply = supply,
            TopCombinations = combos
        };
    }

    public async Task<CandidateMarketInsightDto> GetCandidateInsightAsync(int userId)
    {
        var profile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de candidato no encontrado.");

        var mySkills = await _context.CandidateSkills
            .Where(cs => cs.CandidateId == profile.Id)
            .Select(cs => new { SkillName = cs.Skill.Name, cs.Level })
            .ToListAsync();

        var mySkillNames = mySkills
            .Select(s => s.SkillName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var (demand, supply, combos) = await FetchMarketDataAsync();

        var topDemandWithPresence = demand.Select(d =>
        {
            var mine = mySkills.FirstOrDefault(
                s => string.Equals(s.SkillName, d.SkillName, StringComparison.OrdinalIgnoreCase));
            return new DemandSkillWithPresenceDto
            {
                SkillName      = d.SkillName,
                CategoryName   = d.CategoryName,
                OfferCount     = d.OfferCount,
                CandidateHasSkill = mine is not null,
                CandidateLevel = mine?.Level
            };
        }).ToList();

        var combosWithPresence = combos.Select(c => new ComboWithPresenceDto
        {
            SkillA          = c.SkillA,
            SkillB          = c.SkillB,
            OfferCount      = c.OfferCount,
            CandidateHasA   = mySkillNames.Contains(c.SkillA),
            CandidateHasB   = mySkillNames.Contains(c.SkillB),
            CandidateHasBoth = mySkillNames.Contains(c.SkillA) && mySkillNames.Contains(c.SkillB)
        }).ToList();

        return new CandidateMarketInsightDto
        {
            TopDemand        = topDemandWithPresence,
            TopSupply        = supply,
            TopCombinations  = combosWithPresence,
            SkillsInDemand   = demand.Where(d => mySkillNames.Contains(d.SkillName)).Select(d => d.SkillName).ToList(),
            SkillGaps        = demand.Where(d => !mySkillNames.Contains(d.SkillName)).Select(d => d.SkillName).ToList()
        };
    }

    private async Task<(List<SkillDemandDto> Demand, List<SkillSupplyDto> Supply, List<SkillComboDto> Combos)>
        FetchMarketDataAsync()
    {
        // Traer offer_skills con nombre de skill y categoría (EF Core traduce la navegación)
        var offerSkillsFlat = await _context.OfferSkills
            .Select(os => new
            {
                os.OfferId,
                SkillName    = os.Skill.Name,
                CategoryName = os.Skill.Category.Name
            })
            .ToListAsync();

        var topDemand = offerSkillsFlat
            .GroupBy(x => new { x.SkillName, x.CategoryName })
            .Select(g => new SkillDemandDto
            {
                SkillName    = g.Key.SkillName,
                CategoryName = g.Key.CategoryName,
                OfferCount   = g.Select(x => x.OfferId).Distinct().Count()
            })
            .OrderByDescending(x => x.OfferCount)
            .Take(10)
            .ToList();

        var candidateSkillsFlat = await _context.CandidateSkills
            .Select(cs => new
            {
                cs.CandidateId,
                SkillName    = cs.Skill.Name,
                CategoryName = cs.Skill.Category.Name
            })
            .ToListAsync();

        var topSupply = candidateSkillsFlat
            .GroupBy(x => new { x.SkillName, x.CategoryName })
            .Select(g => new SkillSupplyDto
            {
                SkillName      = g.Key.SkillName,
                CategoryName   = g.Key.CategoryName,
                CandidateCount = g.Select(x => x.CandidateId).Distinct().Count()
            })
            .OrderByDescending(x => x.CandidateCount)
            .Take(10)
            .ToList();

        var topCombinations = ComputeTopCombinations(
            offerSkillsFlat.Select(x => (x.OfferId, x.SkillName)), 10);

        return (topDemand, topSupply, topCombinations);
    }

    private static List<SkillComboDto> ComputeTopCombinations(
        IEnumerable<(int OfferId, string SkillName)> flat, int take)
    {
        var byOffer = flat
            .GroupBy(x => x.OfferId)
            .Select(g => g.Select(x => x.SkillName).Order().ToList())
            .Where(skills => skills.Count >= 2)
            .ToList();

        var counts = new Dictionary<(string A, string B), int>();
        foreach (var skills in byOffer)
            for (int i = 0; i < skills.Count; i++)
                for (int j = i + 1; j < skills.Count; j++)
                {
                    var key = (skills[i], skills[j]);
                    counts[key] = counts.GetValueOrDefault(key) + 1;
                }

        return counts
            .OrderByDescending(kv => kv.Value)
            .Take(take)
            .Select(kv => new SkillComboDto
            {
                SkillA     = kv.Key.A,
                SkillB     = kv.Key.B,
                OfferCount = kv.Value
            })
            .ToList();
    }
}
