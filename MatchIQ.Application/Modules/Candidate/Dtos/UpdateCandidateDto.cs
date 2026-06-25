namespace MatchIQ.Application.Modules.Candidate.Dtos;

public class UpdateCandidateDto
{
    public int? ExperienceYears { get; set; }
    public string? Seniority { get; set; }
    public string? EnglishLevel { get; set; }
    public string? GithubLink { get; set; }
    public string? LinkedinUrl { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public List<int> CategoryIds { get; set; } = [];
    public List<SkillLevelDto> Skills { get; set; } = [];
}
