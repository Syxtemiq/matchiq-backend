namespace MatchIQ.Application.Modules.Tests.Dtos;

public class SubmissionResultDto
{
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? AiEvaluatedAt { get; set; }
    public List<QuestionResultItemDto> QuestionResults { get; set; } = [];
}

public class QuestionResultItemDto
{
    public int QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public string? Feedback { get; set; }
}
