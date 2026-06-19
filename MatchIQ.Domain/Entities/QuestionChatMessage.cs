namespace MatchIQ.Domain.Entities;

// Mensaje del historial de chat entre el admin y la IA para editar una pregunta
// Role: "admin" (lo que escribió el admin) | "assistant" (respuesta de la IA)
// Al pedir un cambio se manda todo el historial como contexto a la IA
// La IA regenera la pregunta y el resultado se guarda en TestQuestion
public class QuestionChatMessage
{
    // TODO: Id, QuestionId, Role ("admin" | "assistant"), Content, CreatedAt
    // TODO: navegación a TestQuestion
}
