namespace MatchIQ.Application.Modules.Tests.Dtos;

public class CandidateSubmissionDetailDto
{
    public int MatchId { get; set; }
    public string CandidateFullName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string? CandidateGithubLink { get; set; }
    public string? CandidateLinkedinUrl { get; set; }
    public string? CandidateProfilePhotoUrl { get; set; }
    public string CandidatePhoneNumber { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public string? GlobalFeedback { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? AiEvaluatedAt { get; set; }
    public List<QuestionSubmissionDetailDto> Questions { get; set; } = [];
}

public class QuestionSubmissionDetailDto
{
    public int QuestionId { get; set; }
    public int OrderIndex { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;

    // MultipleChoice
    public Dictionary<string, string>? Options { get; set; }
    public string? CorrectAnswer { get; set; }
    public string? SelectedOption { get; set; }

    // CodeChallenge
    public string? FunctionSignature { get; set; }
    public string? ExpectedBehavior { get; set; }
    public string? CodeSubmitted { get; set; }

    // Evaluación IA
    public bool? IsCorrect { get; set; }
    public string? AiFeedback { get; set; }
}
