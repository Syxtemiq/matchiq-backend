using MatchIQ.API.Common;
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

    [HttpPost("{offerId:int}/run")]
    public async Task<IActionResult> RunMatching(int offerId)
    {
        var matches = await _matchingService.RunMatchingAsync(offerId, _currentUser.UserId);
        return Ok(ApiResponse.Ok(matches));
    }

    [HttpGet("{offerId:int}")]
    public async Task<IActionResult> GetMatches(int offerId)
    {
        var matches = await _matchingService.GetMatchesByOfferAsync(_currentUser.UserId, offerId);
        return Ok(ApiResponse.Ok(matches));
    }

    [HttpPost("send-test")]
    public async Task<IActionResult> SendTest([FromBody] SendTestDto dto)
    {
        await _matchingService.SendTestsAsync(_currentUser.UserId, dto);
        return Ok(ApiResponse.Ok("Tests enviados correctamente. Los candidatos recibirán un correo con el enlace."));
    }

    [HttpPost("{matchId:int}/select")]
    public async Task<IActionResult> SelectCandidate(int matchId)
    {
        var match = await _matchingService.SelectCandidateAsync(_currentUser.UserId, matchId);
        return Ok(ApiResponse.Ok(match, "Candidato seleccionado correctamente."));
    }

    [HttpPost("{offerId:int}/reevaluate")]
    public async Task<IActionResult> Reevaluate(int offerId)
    {
        var matches = await _matchingService.ReevaluateAsync(offerId, _currentUser.UserId);
        return Ok(ApiResponse.Ok(matches, "Reevaluación completada."));
    }

    [HttpPost("{matchId:int}/reject")]
    public async Task<IActionResult> RejectCandidate(int matchId)
    {
        await _matchingService.RejectCandidateAsync(_currentUser.UserId, matchId);
        return Ok(ApiResponse.Ok("Candidato rechazado correctamente."));
    }
}
