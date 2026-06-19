namespace MatchIQ.Infrastructure.AI;

// Implementación de IOfferParserService
// FEATURE NUEVA: convierte descripción libre en campos estructurados de oferta
// Incluye en el prompt el catálogo real de categorías y skills de la DB
// para que la IA sugiera IDs reales, no inventados
public class OfferParserService // : IOfferParserService
{
    // TODO: inyectar IConfiguration (para API key), AppDbContext (para catálogo)

    // TODO: ParseFromDescriptionAsync(string rawDescription)
    //       1. carga categorías y skills disponibles desde la DB
    //       2. construye prompt incluyendo el catálogo completo
    //       3. llama OpenAI con response_format: json_object
    //       4. parsea y retorna ParsedOfferResponseDto con los campos sugeridos
    //       5. incluye ConfidenceNote explicando qué detectó
}
