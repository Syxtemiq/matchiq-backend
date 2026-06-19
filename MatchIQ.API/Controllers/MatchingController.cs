namespace MatchIQ.API.Controllers;

// [ApiController]
// [Route("api/matching")]
// [Authorize(Roles = "Company")]
public class MatchingController // : ControllerBase
{
    // TODO: inyectar MatchingService

    // GET api/matching/{offerId}
    // TODO: GetMatchesAsync(int offerId)
    //       retorna el ranking de candidatos con AI insights

    // POST api/matching/send-test
    // TODO: SendTestAsync([FromBody] SendTestDto dto)
    //       envía el test a los candidatos seleccionados por la empresa

    // POST api/matching/{matchId}/select
    // TODO: SelectCandidateAsync(int matchId)
    //       marca al candidato como seleccionado
    //       si se llenaron todas las posiciones → cierra la oferta automáticamente
}
