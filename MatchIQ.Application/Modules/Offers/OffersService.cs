namespace MatchIQ.Application.Modules.Offers;

// Gestión de ofertas laborales
// Equivalente a offers.service.js del backend Node
// Al crear una oferta: guarda la oferta → dispara matching → dispara generación del test
public class OffersService
{
    // TODO: inyectar AppDbContext, IMatchRepository, ITestService, IOfferParserService

    // TODO: ParseFromDescriptionAsync(int userId, ParseOfferDto dto)
    //       FEATURE NUEVA: recibe texto libre → la IA extrae campos estructurados
    //       retorna sugerencias para que el admin confirme antes de guardar

    // TODO: CreateOfferAsync(int userId, CreateOfferDto dto)
    //       valida categorías y skills
    //       inserta oferta + offer_categories + offer_skills (transacción)
    //       llama MatchingService.RunMatchingAsync()
    //       llama TestService.GenerateTestAsync()

    // TODO: GetMyOffersAsync(int userId)

    // TODO: GetOfferByIdAsync(int userId, int offerId)
    //       retorna oferta con categorías, skills y matches

    // TODO: UpdateOfferAsync(int userId, int offerId, UpdateOfferDto dto)
    //       solo si status == Open

    // TODO: UpdateOfferStatusAsync(int userId, int offerId, UpdateOfferStatusDto dto)
    //       si cancela y hay candidatos en TestSent/TestCompleted → retorna warning
    //       el frontend muestra confirmación, luego llama ForceCancelAsync

    // TODO: ForceCancelAsync(int userId, int offerId)
}
