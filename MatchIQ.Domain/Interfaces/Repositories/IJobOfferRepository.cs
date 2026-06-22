using MatchIQ.Domain.Entities;

namespace MatchIQ.Domain.Interfaces.Repositories;

// Contrato de acceso a datos para ofertas
// Las implementaciones concretas usan EF Core + LINQ en Infrastructure
public interface IJobOfferRepository
{
    Task<JobOffer?> GetByIdAsync(int id);
    Task<IEnumerable<JobOffer>> GetByCompanyAsync(int companyId);
    Task<JobOffer> CreateAsync(JobOffer offer);
    Task UpdateAsync(JobOffer offer);
}
