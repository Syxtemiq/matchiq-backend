using MatchIQ.Domain.Enums;

namespace MatchIQ.Domain.Entities;

// Pregunta individual dentro de un test
// Tipos: MultipleChoice (opciones A-D) o CodeChallenge (el candidato escribe código)
// El admin puede solicitar cambios a la IA via QuestionChatMessage
// La IA regenera solo esta pregunta, sin tocar las demás
// Gorilla questions: preguntas con distractor oculto para medir atención
public class TestQuestion
{
    public int Id { get; set; }
    public int TestId { get; set; }
    public int OrderIndex { get; set; }
    public QuestionType QuestionType { get; set; }

    public string QuestionText { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public bool IsGorilla { get; set; }
    public string? GorillaHint { get; set; }

    public string? OptionsJson { get; set; }   // para MultipleChoice: {"A":..,"B":..,"C":..,"D":..}
    public string? CorrectAnswer { get; set; }

    public string? Language { get; set; }            // para CodeChallenge: "python", "javascript", etc.
    public string? FunctionSignature { get; set; }   // ej "def calcular_cobro(horas, minutos):"
    public string? ExampleInput { get; set; }
    public string? ExpectedBehavior { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Test Test { get; set; } = null!;
    public ICollection<QuestionChatMessage> QuestionChatMessages { get; set; } = new List<QuestionChatMessage>();
}
