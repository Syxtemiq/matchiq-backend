namespace MatchIQ.Application.Modules.Tests.Dtos;

public class ProctoringReportDto
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime Inicio { get; set; }
    public DateTime? Fin { get; set; }
    public int? TotalFramesProcesados { get; set; }
    public int TotalEventos { get; set; }
    public List<ProctoringEventDto> Eventos { get; set; } = [];
}

public class ProctoringEventDto
{
    public string Tipo { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public string? Evidencia { get; set; }
    public DateTime Timestamp { get; set; }
}
