using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Offers.Dtos;

namespace MatchIQ.Infrastructure.AI;

public class OfferParserService : IOfferParserService
{
    // TODO: inyectar IConfiguration (para API key), AppDbContext (para catálogo de categorías y skills)

    public async Task<ParsedOfferResponseDto> ParseFromDescriptionAsync(string rawDescription)
    {
        throw new NotImplementedException();
    }
}
