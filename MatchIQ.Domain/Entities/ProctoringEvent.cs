namespace MatchIQ.Domain.Entities;

public class ProctoringEvent
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public string? Evidencia { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime CreatedAt { get; set; }

    public ProctoringSession Session { get; set; } = null!;
}
