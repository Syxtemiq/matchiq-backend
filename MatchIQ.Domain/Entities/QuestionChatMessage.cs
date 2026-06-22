using MatchIQ.Domain.Enums;

namespace MatchIQ.Domain.Entities;

// Mensaje del historial de chat entre el admin y la IA para editar una pregunta
// Al pedir un cambio se manda todo el historial como contexto a la IA
// La IA regenera la pregunta y el resultado se guarda en TestQuestion
public class QuestionChatMessage
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public ChatRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public TestQuestion TestQuestion { get; set; } = null!;
}
