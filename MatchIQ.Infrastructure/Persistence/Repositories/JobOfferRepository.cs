using MatchIQ.Application.Common.Interfaces.Repositories;
using MatchIQ.Domain.Entities;
using MatchIQ.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatchIQ.Infrastructure.Persistence.Repositories;

public class JobOfferRepository : IJobOfferRepository
{
    private readonly AppDbContext _context;

    public JobOfferRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<JobOffer?> GetByIdAsync(int id) =>
        await _context.JobOffers
            .Include(o => o.OfferCategories).ThenInclude(oc => oc.Category)
            .Include(o => o.OfferSkills).ThenInclude(os => os.Skill)
            .Include(o => o.PricingTier)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<IEnumerable<JobOffer>> GetByCompanyAsync(int companyId) =>
        await _context.JobOffers
            .Include(o => o.OfferCategories).ThenInclude(oc => oc.Category)
            .Include(o => o.OfferSkills).ThenInclude(os => os.Skill)
            .Include(o => o.PricingTier)
            .Where(o => o.CompanyId == companyId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<JobOffer> CreateAsync(JobOffer offer)
    {
        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync();
        return offer;
    }

    public async Task UpdateAsync(JobOffer offer)
    {
        _context.JobOffers.Update(offer);
        await _context.SaveChangesAsync();
    }
}
