namespace MatchIQ.Domain.Entities;

// Paquete escalonado de precio según cantidad de candidatos a testear
public class PricingTier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MinCandidates { get; set; }
    public int MaxCandidates { get; set; }
    public decimal PriceCop { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<JobOffer> JobOffers { get; set; } = new List<JobOffer>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
