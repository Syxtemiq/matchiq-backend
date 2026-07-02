using MatchIQ.Domain.Enums;

namespace MatchIQ.Domain.Entities;

// Test técnico asociado a una oferta
// Generado automáticamente por la IA al crear la oferta
// La primera pregunta siempre es de tipo CodeChallenge
// El admin puede editar preguntas dialogando con la IA (ver QuestionChatMessage)
public class Test
{
    public int Id { get; set; }
    public int OfferId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TimeLimitMinutes { get; set; } = 30;
    public TestLanguage TestLanguage { get; set; } = TestLanguage.Spanish;
    public DateTime CreatedAt { get; set; }

    public JobOffer JobOffer { get; set; } = null!;
    public ICollection<TestQuestion> TestQuestions { get; set; } = new List<TestQuestion>();
    public ICollection<TestSubmission> TestSubmissions { get; set; } = new List<TestSubmission>();
}
