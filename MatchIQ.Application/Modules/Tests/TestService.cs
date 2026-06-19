namespace MatchIQ.Application.Modules.Tests;

// Gestión de tests técnicos
// Equivalente a gorilla.test.service.js y gorilla.submission.service.js del Node
public class TestService
{
    // TODO: inyectar ITestRepository, IAIService, AppDbContext

    // TODO: GenerateTestAsync(int offerId, bool forceRegenerate = false)
    //       llama IAIService para generar preguntas
    //       guarda cada pregunta como fila en TestQuestion (no como JSON blob)
    //       la primera pregunta siempre es CodeChallenge en el lenguaje de la oferta

    // TODO: GetTestForCandidateAsync(int offerId)
    //       retorna preguntas SIN correct_answer ni explanation

    // TODO: GetFullTestAsync(int offerId)
    //       retorna preguntas CON respuestas correctas (solo admin empresa)

    // TODO: SubmitAnswersAsync(int testId, int candidateId, SubmitAnswersDto dto)
    //       guarda las respuestas, llama IA para evaluar, actualiza score y feedback

    // TODO: GetSubmissionResultAsync(int testId, int candidateId)
}
