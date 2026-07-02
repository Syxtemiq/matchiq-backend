using System.ComponentModel.DataAnnotations;

namespace MatchIQ.Application.Modules.Candidate.Dtos;

public class UpdateCandidateDto
{
    [Range(0, int.MaxValue, ErrorMessage = "Los años de experiencia no pueden ser negativos.")]
    public int? ExperienceYears { get; set; }

    [RegularExpression(@"(?i)^(junior|mid|senior)$", ErrorMessage = "El seniority debe ser junior, mid o senior.")]
    public string? Seniority { get; set; }

    [RegularExpression(@"(?i)^(A1|A2|B1|B2|C1|C2)$", ErrorMessage = "El nivel de inglés debe ser A1, A2, B1, B2, C1 o C2.")]
    public string? EnglishLevel { get; set; }

    public string? GithubLink { get; set; }
    public string? LinkedinUrl { get; set; }
    public string? ProfilePhotoUrl { get; set; }

    [RegularExpression(@"^\+?[0-9\s\-()]{7,20}$", ErrorMessage = "El número de teléfono no es válido.")]
    public string? PhoneNumber { get; set; }
    public List<int> CategoryIds { get; set; } = [];
    public List<SkillLevelDto> Skills { get; set; } = [];
}
