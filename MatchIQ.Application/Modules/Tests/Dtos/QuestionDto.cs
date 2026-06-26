namespace MatchIQ.Application.Modules.Tests.Dtos;

public class QuestionDto
{
    public int Id { get; set; }
    public int OrderIndex { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;

    // Campos ocultos en la vista del candidato (null cuando se llama desde candidate endpoint)
    public bool? IsGorilla { get; set; }
    public string? GorillaHint { get; set; }
    public string? CorrectAnswer { get; set; }
    public string? Explanation { get; set; }

    // MultipleChoice
    public Dictionary<string, string>? Options { get; set; }

    // CodeChallenge
    public string? Language { get; set; }
    public string? FunctionSignature { get; set; }
    public string? ExampleInput { get; set; }
    public string? ExpectedBehavior { get; set; }
}
