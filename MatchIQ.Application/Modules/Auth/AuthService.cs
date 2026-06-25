using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Auth.Dtos;
using MatchIQ.Domain.Entities;
using MatchIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MatchIQ.Application.Modules.Auth;

public class AuthService
{
    private readonly IAppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _config;

    public AuthService(
        IAppDbContext context,
        IJwtService jwtService,
        IEmailService emailService,
        IPasswordHasher passwordHasher,
        IConfiguration config)
    {
        _context = context;
        _jwtService = jwtService;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
        _config = config;
    }

    public async Task RegisterAsync(RegisterDto dto)
    {
        if (dto.Password != dto.ConfirmPassword)
            throw new InvalidOperationException("Las contraseñas no coinciden.");

        if (dto.Role == UserRole.Admin)
            throw new InvalidOperationException("No se puede registrar un administrador.");

        var emailTaken = await _context.Users.AnyAsync(u => u.Email == dto.Email.ToLower());
        if (emailTaken)
            throw new InvalidOperationException("El email ya está registrado.");

        var cedulaTaken = await _context.Users.AnyAsync(u => u.Cedula == dto.Cedula.Trim());
        if (cedulaTaken)
            throw new InvalidOperationException("La cédula ya está registrada.");

        var user = new User
        {
            Email = dto.Email.ToLower().Trim(),
            FullName = dto.FullName.Trim(),
            Cedula = dto.Cedula.Trim(),
            PasswordHash = _passwordHasher.Hash(dto.Password),
            Role = dto.Role,
            IsActive = true,
            EmailVerified = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var code = GenerateSixDigitCode();
        _context.EmailVerifications.Add(new EmailVerification
        {
            UserId = user.Id,
            Code = code,
            Used = false,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        });

        await _context.SaveChangesAsync();
        await _emailService.SendVerificationCodeAsync(user.Email, code);
    }

    public async Task VerifyEmailAsync(VerifyEmailDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());

        if (user is null)
            throw new KeyNotFoundException("Usuario no encontrado.");

        if (user.EmailVerified)
            throw new InvalidOperationException("El email ya fue verificado.");

        var verification = await _context.EmailVerifications
            .Where(v => v.UserId == user.Id && !v.Used && v.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync();

        if (verification is null || verification.Code != dto.Code)
            throw new InvalidOperationException("Código inválido o expirado.");

        verification.Used = true;
        user.EmailVerified = true;

        await _context.SaveChangesAsync();
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());

        if (user is null || user.PasswordHash is null ||
            !_passwordHasher.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        if (!user.EmailVerified)
            throw new UnauthorizedAccessException("Debes verificar tu email antes de iniciar sesión.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Tu cuenta está desactivada. Contacta al soporte.");

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (storedToken is null || storedToken.Revoked || storedToken.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token inválido o expirado.");

        if (!storedToken.User.IsActive)
            throw new UnauthorizedAccessException("Tu cuenta está desactivada.");

        storedToken.Revoked = true;
        await _context.SaveChangesAsync();

        return await BuildAuthResponseAsync(storedToken.User);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());

        // No revelamos si el email existe o no (seguridad anti-enumeración)
        if (user is null || !user.EmailVerified) return;

        var oldTokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.Used && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var old in oldTokens)
            old.Used = true;

        var token = Guid.NewGuid().ToString("N");
        _context.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            Used = false,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });

        await _context.SaveChangesAsync();

        var frontendUrl = _config["App:FrontendUrl"]?.TrimEnd('/') ?? "";
        var resetLink = $"{frontendUrl}/reset-password?token={token}";

        await _emailService.SendPasswordResetAsync(user.Email, resetLink);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
            throw new InvalidOperationException("Las contraseñas no coinciden.");

        var resetToken = await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.Token == dto.Token && !t.Used && t.ExpiresAt > DateTime.UtcNow);

        if (resetToken is null)
            throw new KeyNotFoundException("El enlace de recuperación es inválido o ya expiró.");

        resetToken.User.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        resetToken.Used = true;

        await _context.SaveChangesAsync();
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (storedToken is null || storedToken.Revoked) return;

        storedToken.Revoked = true;
        await _context.SaveChangesAsync();
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(User user)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var rawRefreshToken = _jwtService.GenerateRefreshToken();

        var refreshExpirationDays = int.Parse(
            _config["Jwt:RefreshTokenExpirationDays"] ?? "7");

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = rawRefreshToken,
            Revoked = false,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshExpirationDays)
        });

        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            UserId = user.Id,
            Role = user.Role.ToString(),
            FullName = user.FullName ?? string.Empty,
            EmailVerified = user.EmailVerified
        };
    }

    private static string GenerateSixDigitCode() =>
        Random.Shared.Next(100_000, 999_999).ToString();
}
