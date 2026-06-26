namespace MatchIQ.Application.Common.Dtos;

public class SubmissionEvaluationDto
{
    public decimal Score { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public List<QuestionEvaluationDto> QuestionResults { get; set; } = new();
}

public class QuestionEvaluationDto
{
    public int QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public string? Feedback { get; set; }
}
