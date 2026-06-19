namespace MatchIQ.Domain.Entities;

// Resultado del matching entre una oferta y un candidato
// El porcentaje viene de la función SQL get_candidate_matches()
// La IA agrega un insight cualitativo sobre los top 3
// Etapas: Matched → TestSent → TestCompleted → Selected | Rejected
public class Match
{
    // TODO: Id, OfferId, CandidateId, MatchPercentage, AdjustedScore
    // TODO: Stage (MatchStage enum), AiFeedback (JSON string), CreatedAt, UpdatedAt
    // TODO: navegación a JobOffer y CandidateProfile
}
