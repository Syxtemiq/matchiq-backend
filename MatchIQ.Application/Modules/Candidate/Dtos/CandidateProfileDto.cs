using MatchIQ.Application.Modules.Catalog.Dtos;

namespace MatchIQ.Application.Modules.Candidate.Dtos;

public class CandidateProfileDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? ExperienceYears { get; set; }
    public string? Seniority { get; set; }
    public string? EnglishLevel { get; set; }
    public string? GithubLink { get; set; }
    public string? LinkedinUrl { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool ProfileCompleted { get; set; }
    public List<CategoryDto> Categories { get; set; } = [];
    public List<CandidateSkillDto> Skills { get; set; } = [];
}
