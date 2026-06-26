namespace MatchIQ.Application.Modules.Tests.Dtos;

public class EditQuestionResponseDto
{
    public QuestionDto UpdatedQuestion { get; set; } = null!;
    public string AssistantMessage { get; set; } = string.Empty;
}
