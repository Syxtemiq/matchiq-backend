namespace MatchIQ.Domain.Entities;

// Tabla pivot: skills del candidato con nivel de dominio (1 a 5)
public class CandidateSkill
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public int SkillId { get; set; }
    public int? Level { get; set; }

    public CandidateProfile CandidateProfile { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
