namespace MatchIQ.Domain.Entities;

// Tabla pivot: categorías requeridas por una oferta
public class OfferCategory
{
    public int Id { get; set; }
    public int OfferId { get; set; }
    public int CategoryId { get; set; }

    public JobOffer JobOffer { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
