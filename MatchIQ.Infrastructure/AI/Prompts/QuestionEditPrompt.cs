namespace MatchIQ.Infrastructure.AI.Prompts;

// Prompt para regenerar una pregunta basándose en el historial del chat admin-IA
// El historial completo de la conversación se incluye como contexto
// La IA retorna solo la pregunta modificada en el mismo formato JSON
public static class QuestionEditPrompt
{
    // TODO: BuildSystemPrompt() → string
    //       describe el rol de la IA como editor de preguntas técnicas

    // TODO: BuildMessages(TestQuestion currentQuestion, List<QuestionChatMessage> history, string newMessage)
    //       retorna lista de mensajes en formato OpenAI (system + historial + nuevo)
}
