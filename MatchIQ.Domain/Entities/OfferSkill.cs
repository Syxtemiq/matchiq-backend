namespace MatchIQ.Domain.Entities;

// Tabla pivot: skills requeridos por una oferta
public class OfferSkill
{
    public int Id { get; set; }
    public int OfferId { get; set; }
    public int SkillId { get; set; }

    public JobOffer JobOffer { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
