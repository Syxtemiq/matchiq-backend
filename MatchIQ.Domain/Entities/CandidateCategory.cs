namespace MatchIQ.Domain.Entities;

// Tabla pivot: categorías que domina un candidato
public class CandidateCategory
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public int CategoryId { get; set; }

    public CandidateProfile CandidateProfile { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
