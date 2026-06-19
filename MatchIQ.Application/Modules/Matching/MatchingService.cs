namespace MatchIQ.Application.Modules.Matching;

// Orquesta el proceso de matching entre una oferta y los candidatos
// Paso 1: ejecuta la función SQL get_candidate_matches() via EF Core FromSqlRaw
// Paso 2: los top 3 pasan por la IA para insight cualitativo (AIService)
// Paso 3: combina score SQL (90%) + AI fit_score (10%) = adjusted_score
// Equivalente a matching.service.js del backend Node
public class MatchingService
{
    // TODO: inyectar IMatchRepository, IAIService, AppDbContext

    // TODO: RunMatchingAsync(int offerId, int aiTop = 3)
    //       retorna ranking completo + top candidatos con AI feedback

    // TODO: GetMatchesByOfferAsync(int offerId)
    //       retorna el ranking guardado con etapas actuales

    // TODO: SendTestToMatchAsync(int matchId)
    //       cambia stage a TestSent, notifica al candidato por email

    // TODO: SelectCandidateAsync(int matchId)
    //       marca como Selected, si se llenaron todas las posiciones → cierra la oferta
}
