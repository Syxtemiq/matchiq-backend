using MatchIQ.Domain.Enums;

namespace MatchIQ.Application.Common.Dtos;

public class GeneratedQuestionDto
{
    public QuestionType QuestionType { get; set; }
    public int OrderIndex { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public bool IsGorilla { get; set; }
    public string? GorillaHint { get; set; }

    // Solo para MultipleChoice
    public Dictionary<string, string>? Options { get; set; }
    public string? CorrectAnswer { get; set; }

    // Solo para CodeChallenge
    public string? Language { get; set; }
    public string? FunctionSignature { get; set; }
    public string? ExampleInput { get; set; }
    public string? ExpectedBehavior { get; set; }
}
