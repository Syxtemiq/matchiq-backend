using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Catalog.Dtos;
using MatchIQ.Application.Modules.Offers;
using MatchIQ.Application.Modules.Offers.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchIQ.API.Controllers;

[ApiController]
[Route("api/offers")]
[Authorize(Roles = "Company")]
public class OffersController : ControllerBase
{
    private readonly OffersService _offersService;
    private readonly ICurrentUserService _currentUser;
    private readonly IAppDbContext _context;

    public OffersController(OffersService offersService, ICurrentUserService currentUser, IAppDbContext context)
    {
        _offersService = offersService;
        _currentUser = currentUser;
        _context = context;
    }

    [HttpGet("tiers")]
    public async Task<IActionResult> GetTiers()
    {
        var tiers = await _context.PricingTiers
            .Where(t => t.IsActive)
            .OrderBy(t => t.MinCandidates)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.MinCandidates,
                t.MaxCandidates,
                t.PriceCop
            })
            .ToListAsync();

        return Ok(tiers);
    }

    [HttpPost("parse-description")]
    public async Task<IActionResult> ParseDescription([FromBody] ParseOfferDto dto)
    {
        var result = await _offersService.ParseFromDescriptionAsync(dto.RawDescription);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOffer([FromBody] CreateOfferDto dto)
    {
        var offer = await _offersService.CreateOfferAsync(_currentUser.UserId, dto);
        return Created($"/api/offers/{offer.Id}", offer);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOffers()
    {
        var offers = await _offersService.GetMyOffersAsync(_currentUser.UserId);
        return Ok(offers);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOfferById(int id)
    {
        var offer = await _offersService.GetOfferByIdAsync(_currentUser.UserId, id);
        return Ok(offer);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateOffer(int id, [FromBody] UpdateOfferDto dto)
    {
        var offer = await _offersService.UpdateOfferAsync(_currentUser.UserId, id, dto);
        return Ok(offer);
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> CancelOffer(int id)
    {
        var result = await _offersService.CancelOfferAsync(_currentUser.UserId, id);
        return Ok(result);
    }

    [HttpPost("{id:int}/force-cancel")]
    public async Task<IActionResult> ForceCancel(int id)
    {
        await _offersService.ForceCancelAsync(_currentUser.UserId, id);
        return NoContent();
    }
}
