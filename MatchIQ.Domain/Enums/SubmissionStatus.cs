namespace MatchIQ.Domain.Enums;

public enum SubmissionStatus
{
    Pending,    // el candidato aún no ha respondido
    Evaluated,  // la IA ya evaluó la respuesta
    Expired     // el tiempo límite venció sin respuesta
}
