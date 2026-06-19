namespace MatchIQ.Infrastructure.AI;

// Implementación concreta de IAIService usando el SDK de OpenAI para .NET
// Modelo: gpt-4o-mini (configurable por appsettings)
// Todos los prompts se construyen en la carpeta Prompts/
public class OpenAIService // : IAIService
{
    // TODO: inyectar IConfiguration para leer OPENAI_API_KEY y OPENAI_MODEL

    // TODO: GenerateTestAsync(JobOffer offer)
    //       construye prompt con TestGenerationPrompt
    //       llama OpenAI con response_format: json_object
    //       parsea la respuesta y retorna lista de preguntas generadas
    //       la primera pregunta siempre es CodeChallenge

    // TODO: RegenerateQuestionAsync(TestQuestion currentQuestion, List<QuestionChatMessage> history, string adminMessage)
    //       construye el historial como mensajes del chat de OpenAI
    //       agrega el mensaje nuevo del admin
    //       la IA retorna solo la pregunta regenerada en JSON
    //       retorna la pregunta actualizada

    // TODO: EvaluateCandidateAsync(JobOffer offer, MatchResult candidate)
    //       prompt de EvaluationPrompt para insight cualitativo del matching
    //       retorna fit_score, insight, strengths, opportunities, recommendation

    // TODO: EvaluateSubmissionAsync(Test test, TestSubmission submission)
    //       evalúa código de la CodeChallenge + respuestas de múltiple opción
    //       retorna score (0-100), feedback detallado y resultado por pregunta
}
