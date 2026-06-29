namespace MatchIQ.Application.Modules.Tests.Dtos;

public class TestDto
{
    public int Id { get; set; }
    public int OfferId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TimeLimitMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<QuestionDto> Questions { get; set; } = [];
}
