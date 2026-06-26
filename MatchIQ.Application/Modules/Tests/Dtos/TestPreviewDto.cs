namespace MatchIQ.Application.Modules.Tests.Dtos;

public class TestPreviewDto
{
    public int TestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TimeLimitMinutes { get; set; }
    public int TotalQuestions { get; set; }
    public int MultipleChoiceCount { get; set; }
    public int CodeChallengeCount { get; set; }
}
