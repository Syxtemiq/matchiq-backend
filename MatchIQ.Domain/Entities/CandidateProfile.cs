using MatchIQ.Domain.Enums;

namespace MatchIQ.Domain.Entities;

// Perfil del candidato: experiencia, seniority, nivel de inglés
// Relaciones: categorías (CandidateCategory) y skills (CandidateSkill)
public class CandidateProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ExperienceYears { get; set; }
    public Seniority? Seniority { get; set; }
    public EnglishLevel? EnglishLevel { get; set; }
    public string? GithubLink { get; set; }
    public string? ProfilePhotoUrl { get; set; } // URL del storage externo (S3/Cloudinary/Azure Blob)
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<CandidateCategory> CandidateCategories { get; set; } = new List<CandidateCategory>();
    public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
    public ICollection<TestSubmission> TestSubmissions { get; set; } = new List<TestSubmission>();
}
