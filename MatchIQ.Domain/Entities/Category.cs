namespace MatchIQ.Domain.Entities;

// Categoría técnica: FrontEnd, BackEnd, FullStack, DevOps, QA, UX/UI, Databases
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    public ICollection<CandidateCategory> CandidateCategories { get; set; } = new List<CandidateCategory>();
    public ICollection<OfferCategory> OfferCategories { get; set; } = new List<OfferCategory>();
}
