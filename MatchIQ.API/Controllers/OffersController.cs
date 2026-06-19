namespace MatchIQ.API.Controllers;

// [ApiController]
// [Route("api/offers")]
// [Authorize(Roles = "Company")]
public class OffersController // : ControllerBase
{
    // TODO: inyectar OffersService

    // POST api/offers/parse-description
    // FEATURE NUEVA: recibe texto libre, retorna campos pre-llenados por la IA
    // TODO: ParseDescriptionAsync([FromBody] ParseOfferDto dto)

    // POST api/offers
    // TODO: CreateOfferAsync([FromBody] CreateOfferDto dto)
    //       crea la oferta, dispara matching y generación del test

    // GET api/offers
    // TODO: GetMyOffersAsync()

    // GET api/offers/{id}
    // TODO: GetOfferByIdAsync(int id)

    // PUT api/offers/{id}
    // TODO: UpdateOfferAsync(int id, [FromBody] UpdateOfferDto dto)

    // PATCH api/offers/{id}/status
    // TODO: UpdateStatusAsync(int id, [FromBody] UpdateOfferStatusDto dto)
    //       puede retornar warning si hay candidatos en proceso al cancelar

    // POST api/offers/{id}/force-cancel
    // TODO: ForceCancelAsync(int id)
    //       cancelación confirmada después del warning
}
