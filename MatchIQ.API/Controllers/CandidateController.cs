using MatchIQ.API.Common;
using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Candidate;
using MatchIQ.Application.Modules.Candidate.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatchIQ.API.Controllers;

[ApiController]
[Route("api/candidate")]
[Authorize(Roles = "Candidate")]
public class CandidateController : ControllerBase
{
    private readonly CandidateService _candidateService;
    private readonly ICurrentUserService _currentUser;

    public CandidateController(CandidateService candidateService, ICurrentUserService currentUser)
    {
        _candidateService = candidateService;
        _currentUser = currentUser;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _candidateService.GetProfileAsync(_currentUser.UserId);
        return Ok(ApiResponse.Ok(profile));
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateCandidateDto dto)
    {
        var profile = await _candidateService.UpdateProfileAsync(_currentUser.UserId, dto);
        return Ok(ApiResponse.Ok(profile, "Perfil actualizado correctamente."));
    }
}
