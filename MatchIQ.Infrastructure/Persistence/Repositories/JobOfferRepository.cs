namespace MatchIQ.Infrastructure.Persistence.Repositories;

// Implementación concreta del repositorio de ofertas usando EF Core + LINQ
public class JobOfferRepository // : IJobOfferRepository
{
    // TODO: inyectar AppDbContext

    // TODO: GetByIdAsync → _context.JobOffers.Include(categorías).Include(skills)
    //                                         .FirstOrDefaultAsync(o => o.Id == id)

    // TODO: GetByCompanyAsync → _context.JobOffers
    //                                    .Where(o => o.CompanyId == companyId)
    //                                    .OrderByDescending(o => o.CreatedAt)
    //                                    .ToListAsync()

    // TODO: CreateAsync → _context.JobOffers.Add(offer) + SaveChangesAsync
    // TODO: UpdateAsync → _context.SaveChangesAsync
}
