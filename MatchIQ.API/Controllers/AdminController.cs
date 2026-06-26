using MatchIQ.API.Common;
using MatchIQ.Application.Modules.Admin;
using MatchIQ.Application.Modules.Admin.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatchIQ.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto)
    {
        var user = await _adminService.CreateAdminAsync(dto);
        return Ok(ApiResponse.Ok(user, "Administrador creado correctamente."));
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null)
    {
        var users = await _adminService.GetAllUsersAsync(role, isActive);
        return Ok(ApiResponse.Ok(users));
    }

    [HttpGet("users/{userId:int}")]
    public async Task<IActionResult> GetUserById(int userId)
    {
        var user = await _adminService.GetUserByIdAsync(userId);
        return Ok(ApiResponse.Ok(user));
    }

    [HttpPatch("users/{userId:int}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(int userId)
    {
        var user = await _adminService.ToggleUserStatusAsync(userId);
        var estado = user.IsActive ? "activada" : "desactivada";
        return Ok(ApiResponse.Ok(user, $"Cuenta {estado} correctamente."));
    }

    [HttpDelete("users/{userId:int}")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        await _adminService.DeleteUserAsync(userId);
        return Ok(ApiResponse.Ok("Usuario eliminado correctamente."));
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _adminService.GetStatsAsync();
        return Ok(ApiResponse.Ok(stats));
    }
}
