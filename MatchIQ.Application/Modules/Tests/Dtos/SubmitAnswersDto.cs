using System.ComponentModel.DataAnnotations;

namespace MatchIQ.Application.Modules.Tests.Dtos;

public class SubmitAnswersDto
{
    [Required]
    public List<AnswerItemDto> Answers { get; set; } = [];
}

public class AnswerItemDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El QuestionId debe ser un valor positivo.")]
    public int QuestionId { get; set; }

    [RegularExpression(@"^[ABCD]$", ErrorMessage = "La opción seleccionada debe ser A, B, C o D.")]
    public string? SelectedOption { get; set; }

    public string? CodeSubmitted { get; set; }
}
