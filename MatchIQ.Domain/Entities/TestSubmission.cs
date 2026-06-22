using MatchIQ.Domain.Enums;

namespace MatchIQ.Domain.Entities;

// Respuesta de un candidato a un test completo
// La IA evalúa el código de la CodeChallenge y las respuestas de MultipleChoice
// Estados: Pending → Evaluated | Expired
public class TestSubmission
{
    public int Id { get; set; }
    public int TestId { get; set; }
    public int CandidateId { get; set; }
    public string? AnswersJson { get; set; }
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
    public DateTime? StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? AiEvaluatedAt { get; set; }
    public DateTime? Deadline { get; set; }

    public Test Test { get; set; } = null!;
    public CandidateProfile CandidateProfile { get; set; } = null!;
}
