namespace MatchIQ.Domain.Interfaces;

// Contrato para el servicio de IA
// La implementación concreta vive en Infrastructure/AI/OpenAIService.cs
// Cambiar de OpenAI a otro proveedor solo requiere cambiar esa implementación
public interface IAIService
{
    // Genera el test completo para una oferta
    // La primera pregunta siempre será CodeChallenge en el lenguaje de la oferta
    // TODO: Task<GeneratedTestDto> GenerateTestAsync(...)

    // Regenera una sola pregunta basándose en el historial de chat del admin
    // TODO: Task<GeneratedQuestionDto> RegenerateQuestionAsync(...)

    // Evalúa el insight cualitativo de un candidato (top 3 del matching)
    // TODO: Task<CandidateInsightDto> EvaluateCandidateAsync(...)

    // Evalúa la submission de un candidato (código + respuestas múltiple opción)
    // TODO: Task<SubmissionEvaluationDto> EvaluateSubmissionAsync(...)
}
