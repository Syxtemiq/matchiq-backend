using MatchIQ.Domain.Entities;

namespace MatchIQ.Application.Common.Interfaces.Repositories;

public interface IJobOfferRepository
{
    Task<JobOffer?> GetByIdAsync(int id);
    Task<IEnumerable<JobOffer>> GetByCompanyAsync(int companyId);
    Task<JobOffer> CreateAsync(JobOffer offer);
    Task UpdateAsync(JobOffer offer);
}
