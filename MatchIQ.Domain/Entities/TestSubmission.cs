namespace MatchIQ.Domain.Entities;

// Respuesta de un candidato a un test completo
// La IA evalúa el código de la CodeChallenge y las respuestas de MultipleChoice
// Estados: Pending → Evaluated | Expired
public class TestSubmission
{
    // TODO: Id, TestId, CandidateId
    // TODO: AnswersJson (respuestas del candidato por pregunta)
    // TODO: Score (0-100), Feedback (texto de la IA), Status (SubmissionStatus enum)
    // TODO: StartedAt, SubmittedAt, AiEvaluatedAt
    // TODO: navegación a Test y CandidateProfile
}
