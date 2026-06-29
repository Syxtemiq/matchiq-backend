using MatchIQ.API.Common;
using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatchIQ.API.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly MarketService _marketService;
    private readonly ICurrentUserService _currentUser;

    public AnalyticsController(MarketService marketService, ICurrentUserService currentUser)
    {
        _marketService = marketService;
        _currentUser = currentUser;
    }

    [HttpGet("market")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMarket()
    {
        var data = await _marketService.GetMarketAsync();
        return Ok(ApiResponse.Ok(data));
    }

    [HttpGet("market/my-insights")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetMyInsights()
    {
        var insight = await _marketService.GetCandidateInsightAsync(_currentUser.UserId);
        return Ok(ApiResponse.Ok(insight));
    }
}
