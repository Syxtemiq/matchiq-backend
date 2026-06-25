namespace MatchIQ.Application.Modules.Tests.Dtos;

public class SubmitAnswersDto
{
    public List<AnswerItemDto> Answers { get; set; } = [];
}

public class AnswerItemDto
{
    public int QuestionId { get; set; }
    public string? SelectedOption { get; set; }  // "A", "B", "C" o "D" para MultipleChoice
    public string? CodeSubmitted { get; set; }    // código fuente para CodeChallenge
}
