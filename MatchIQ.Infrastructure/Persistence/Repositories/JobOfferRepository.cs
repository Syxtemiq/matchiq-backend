using MatchIQ.Domain.Entities;
using MatchIQ.Application.Common.Interfaces.Repositories;

namespace MatchIQ.Infrastructure.Persistence.Repositories;

// Implementación concreta del repositorio de ofertas usando EF Core + LINQ
public class JobOfferRepository : IJobOfferRepository
{
    public async Task<JobOffer?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<JobOffer>> GetByCompanyAsync(int companyId)
    {
        throw new NotImplementedException();
    }

    public async Task<JobOffer> CreateAsync(JobOffer offer)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(JobOffer offer)
    {
        throw new NotImplementedException();
    }
}
