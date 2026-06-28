namespace MatchIQ.Domain.Entities;

public class ProctoringSession
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string UsuarioId { get; set; } = string.Empty;
    public int? SubmissionId { get; set; }
    public DateTime Inicio { get; set; }
    public DateTime? Fin { get; set; }
    public int? TotalFramesProcesados { get; set; }
    public decimal? IntegrityScore { get; set; }
    public string? IntegritySummary { get; set; }
    public DateTime CreatedAt { get; set; }

    public TestSubmission? Submission { get; set; }
    public ICollection<ProctoringEvent> Events { get; set; } = new List<ProctoringEvent>();
}
