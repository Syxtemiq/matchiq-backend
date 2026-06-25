namespace MatchIQ.Application.Common.Dtos;

public class CandidateInsightDto
{
    public decimal FitScore { get; set; }
    public string Insight { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Opportunities { get; set; } = new();
    public string Recommendation { get; set; } = string.Empty;
}
