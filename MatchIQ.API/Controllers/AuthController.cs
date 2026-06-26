using MatchIQ.API.Common;
using MatchIQ.Application.Modules.Auth;
using MatchIQ.Application.Modules.Auth.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MatchIQ.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [EnableRateLimiting("auth-strict")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        await _authService.RegisterAsync(dto);
        return Ok(ApiResponse.Ok("Registro exitoso. Revisa tu email e ingresa el código de verificación."));
    }

    [EnableRateLimiting("auth-general")]
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
    {
        await _authService.VerifyEmailAsync(dto);
        return Ok(ApiResponse.Ok("Email verificado. Ya puedes iniciar sesión y completar tu perfil."));
    }

    [EnableRateLimiting("auth-strict")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var response = await _authService.LoginAsync(dto);
        return Ok(ApiResponse.Ok(response));
    }

    [EnableRateLimiting("auth-general")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        var response = await _authService.RefreshTokenAsync(dto.RefreshToken);
        return Ok(ApiResponse.Ok(response));
    }

    [EnableRateLimiting("auth-strict")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await _authService.ForgotPasswordAsync(dto);
        return Ok(ApiResponse.Ok("Si el email existe, recibirás un enlace para restablecer tu contraseña."));
    }

    [EnableRateLimiting("auth-strict")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await _authService.ResetPasswordAsync(dto);
        return Ok(ApiResponse.Ok("Contraseña actualizada correctamente."));
    }

    [EnableRateLimiting("auth-general")]
    [HttpPost("google")]
    public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleLoginDto dto)
    {
        var response = await _authService.LoginWithGoogleAsync(dto);
        return Ok(ApiResponse.Ok(response));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        await _authService.LogoutAsync(dto.RefreshToken);
        return Ok(ApiResponse.Ok("Sesión cerrada."));
    }
}
