using MatchIQ.Application.Modules.Offers.Dtos;

namespace MatchIQ.Application.Common.Interfaces;

public interface IOfferParserService
{
    
    Task<ParsedOfferResponseDto> ParseFromDescriptionAsync(string rawDescription);
}
