using MatchIQ.Application.Modules.Admin;
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

    /// <summary>
    /// Lista todos los usuarios. Filtrables por rol y estado.
    /// GET /api/admin/users?role=Candidate&isActive=true
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null)
    {
        var users = await _adminService.GetAllUsersAsync(role, isActive);
        return Ok(users);
    }

    /// <summary>
    /// Detalle de un usuario específico.
    /// GET /api/admin/users/{userId}
    /// </summary>
    [HttpGet("users/{userId:int}")]
    public async Task<IActionResult> GetUserById(int userId)
    {
        var user = await _adminService.GetUserByIdAsync(userId);
        return Ok(user);
    }

    /// <summary>
    /// Activa o desactiva una cuenta (toggle is_active).
    /// PATCH /api/admin/users/{userId}/toggle-status
    /// </summary>
    [HttpPatch("users/{userId:int}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(int userId)
    {
        var user = await _adminService.ToggleUserStatusAsync(userId);
        var estado = user.IsActive ? "activada" : "desactivada";
        return Ok(new { message = $"Cuenta {estado} correctamente.", user });
    }

    /// <summary>
    /// Elimina un usuario y sus datos en cascada.
    /// DELETE /api/admin/users/{userId}
    /// </summary>
    [HttpDelete("users/{userId:int}")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        await _adminService.DeleteUserAsync(userId);
        return Ok(new { message = "Usuario eliminado correctamente." });
    }

    /// <summary>
    /// Estadísticas generales del sistema.
    /// GET /api/admin/stats
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _adminService.GetStatsAsync();
        return Ok(stats);
    }
}
