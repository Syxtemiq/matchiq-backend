namespace MatchIQ.Domain.Entities;

// Perfil de la empresa: nombre, usuario asociado
// Una empresa tiene muchas ofertas (JobOffer)
public class CompanyProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? CompanyName { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<JobOffer> JobOffers { get; set; } = new List<JobOffer>();
}
