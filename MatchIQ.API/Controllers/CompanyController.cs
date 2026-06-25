using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Company;
using MatchIQ.Application.Modules.Company.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatchIQ.API.Controllers;

[ApiController]
[Route("api/company")]
[Authorize(Roles = "Company")]
public class CompanyController : ControllerBase
{
    private readonly CompanyService _companyService;
    private readonly ICurrentUserService _currentUser;

    public CompanyController(CompanyService companyService, ICurrentUserService currentUser)
    {
        _companyService = companyService;
        _currentUser = currentUser;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _companyService.GetProfileAsync(_currentUser.UserId);
        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateCompanyDto dto)
    {
        var profile = await _companyService.UpdateProfileAsync(_currentUser.UserId, dto);
        return Ok(profile);
    }
}
