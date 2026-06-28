using MatchIQ.API.Common;
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
    private readonly IReportService _reportService;

    public CompanyController(CompanyService companyService, ICurrentUserService currentUser, IReportService reportService)
    {
        _companyService = companyService;
        _currentUser = currentUser;
        _reportService = reportService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _companyService.GetDashboardAsync(_currentUser.UserId);
        return Ok(ApiResponse.Ok(dashboard));
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _companyService.GetProfileAsync(_currentUser.UserId);
        return Ok(ApiResponse.Ok(profile));
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateCompanyDto dto)
    {
        var profile = await _companyService.UpdateProfileAsync(_currentUser.UserId, dto);
        return Ok(ApiResponse.Ok(profile, "Perfil actualizado correctamente."));
    }

    [HttpGet("report")]
    public async Task<IActionResult> DownloadReport()
    {
        var bytes = await _reportService.GenerateCompanyReportAsync(_currentUser.UserId);
        var fileName = $"matchiq-reporte-empresa-{DateTime.UtcNow:yyyy-MM-dd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
