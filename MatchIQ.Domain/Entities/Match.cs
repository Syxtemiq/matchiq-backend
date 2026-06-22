using MatchIQ.Domain.Enums;

namespace MatchIQ.Domain.Entities;

// Resultado del matching entre una oferta y un candidato
// El porcentaje viene de la función SQL get_candidate_matches()
// La IA agrega un insight cualitativo sobre los top 3
// Etapas: Matched → TestSent → TestCompleted → Selected | Rejected
public class Match
{
    public int Id { get; set; }
    public int OfferId { get; set; }
    public int CandidateId { get; set; }
    public decimal? MatchPercentage { get; set; }
    public decimal? AdjustedScore { get; set; }     // 90% SQL + 10% IA
    public string? AiFeedback { get; set; }          // JSON con el insight cualitativo de la IA para el top 3
    public MatchStage Stage { get; set; } = MatchStage.Matched;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public JobOffer JobOffer { get; set; } = null!;
    public CandidateProfile CandidateProfile { get; set; } = null!;
}
