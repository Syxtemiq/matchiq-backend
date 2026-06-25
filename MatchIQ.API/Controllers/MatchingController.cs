using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Matching;
using MatchIQ.Application.Modules.Matching.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatchIQ.API.Controllers;

[ApiController]
[Route("api/matching")]
[Authorize(Roles = "Company")]
public class MatchingController : ControllerBase
{
    private readonly MatchingService _matchingService;
    private readonly ICurrentUserService _currentUser;

    public MatchingController(MatchingService matchingService, ICurrentUserService currentUser)
    {
        _matchingService = matchingService;
        _currentUser = currentUser;
    }

    // Ejecuta la función SQL de matching para una oferta y enriquece con IA.
    // Se llama automáticamente vía webhook de Stripe; también disponible manualmente.
    [HttpPost("{offerId:int}/run")]
    public async Task<IActionResult> RunMatching(int offerId)
    {
        var matches = await _matchingService.RunMatchingAsync(offerId);
        return Ok(matches);
    }

    [HttpGet("{offerId:int}")]
    public async Task<IActionResult> GetMatches(int offerId)
    {
        var matches = await _matchingService.GetMatchesByOfferAsync(_currentUser.UserId, offerId);
        return Ok(matches);
    }

    [HttpPost("send-test")]
    public async Task<IActionResult> SendTest([FromBody] SendTestDto dto)
    {
        await _matchingService.SendTestsAsync(_currentUser.UserId, dto);
        return NoContent();
    }

    [HttpPost("{matchId:int}/select")]
    public async Task<IActionResult> SelectCandidate(int matchId)
    {
        var match = await _matchingService.SelectCandidateAsync(_currentUser.UserId, matchId);
        return Ok(match);
    }
}
