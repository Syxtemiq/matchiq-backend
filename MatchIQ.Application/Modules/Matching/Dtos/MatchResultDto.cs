namespace MatchIQ.Application.Modules.Matching.Dtos;

public class MatchResultDto
{
    public int MatchId { get; set; }
    public int CandidateId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? GithubLink { get; set; }
    public string? LinkedinUrl { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public int? ExperienceYears { get; set; }
    public string? EnglishLevel { get; set; }
    public decimal? MatchPercentage { get; set; }
    public decimal? AdjustedScore { get; set; }
    public string Stage { get; set; } = string.Empty;
    public string? AiInsight { get; set; }
    public List<string> AiStrengths { get; set; } = [];
    public List<string> AiOpportunities { get; set; } = [];
    public string? AiRecommendation { get; set; }
    public List<string> MatchedSkills { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public decimal? TestScore { get; set; }
    public string? TestFeedback { get; set; }
}
