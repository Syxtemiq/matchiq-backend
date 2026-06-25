namespace MatchIQ.Application.Common.Dtos;

public class GeneratedTestDto
{
    public string Title { get; set; } = string.Empty;
    public int TimeLimitMinutes { get; set; }
    public List<GeneratedQuestionDto> Questions { get; set; } = new();
}
