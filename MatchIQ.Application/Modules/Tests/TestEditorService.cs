namespace MatchIQ.Application.Modules.Tests;

// Gestiona el chat entre el admin y la IA para editar preguntas del test
// Cada pregunta tiene su propio historial de conversación
public class TestEditorService
{
    // TODO: inyectar ITestRepository, IAIService

    // TODO: SendMessageAsync(int questionId, int userId, string message)
    //       guarda el mensaje del admin en QuestionChatMessage (role: "admin")
    //       recupera historial completo de esa pregunta
    //       manda historial + pregunta actual a la IA
    //       la IA retorna la pregunta regenerada
    //       guarda respuesta de la IA en QuestionChatMessage (role: "assistant")
    //       actualiza TestQuestion con la nueva versión de la pregunta
    //       retorna pregunta actualizada + mensaje de la IA

    // TODO: GetChatHistoryAsync(int questionId)
    //       retorna historial de mensajes de esa pregunta
}
