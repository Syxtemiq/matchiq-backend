namespace MatchIQ.Domain.Entities;

// Skill específico dentro de una categoría (ej: Python dentro de BackEnd)
public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;
    public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
    public ICollection<OfferSkill> OfferSkills { get; set; } = new List<OfferSkill>();
}
