using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Admin.Dtos;
using MatchIQ.Domain.Entities;
using MatchIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MatchIQ.Application.Modules.Admin;

public class AdminService
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public AdminService(IAppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserSummaryDto> CreateAdminAsync(CreateAdminDto dto)
    {
        if (dto.Password != dto.ConfirmPassword)
            throw new InvalidOperationException("Las contraseñas no coinciden.");

        var emailTaken = await _context.Users.AnyAsync(u => u.Email == dto.Email.ToLower().Trim());
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
            Role = UserRole.Admin,
            IsActive = true,
            EmailVerified = true   // el admin que lo crea ya garantiza la identidad
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserSummaryDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Cedula = user.Cedula,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            EmailVerified = user.EmailVerified,
            CreatedAt = user.CreatedAt,
            ProfileName = null
        };
    }

    public async Task<List<UserSummaryDto>> GetAllUsersAsync(string? role = null, bool? isActive = null)
    {
        var query = _context.Users
            .Include(u => u.CompanyProfile)
            .AsQueryable();

        if (role is not null && Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsedRole))
            query = query.Where(u => u.Role == parsedRole);

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return users.Select(u => new UserSummaryDto
        {
            Id = u.Id,
            Email = u.Email,
            FullName = u.FullName,
            Cedula = u.Cedula,
            Role = u.Role.ToString(),
            IsActive = u.IsActive,
            EmailVerified = u.EmailVerified,
            CreatedAt = u.CreatedAt,
            ProfileName = u.Role == UserRole.Company ? u.CompanyProfile?.CompanyName : null
        }).ToList();
    }

    public async Task<UserSummaryDto> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.CompanyProfile)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException($"Usuario {userId} no encontrado.");

        return new UserSummaryDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Cedula = user.Cedula,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            EmailVerified = user.EmailVerified,
            CreatedAt = user.CreatedAt,
            ProfileName = user.Role == UserRole.Company ? user.CompanyProfile?.CompanyName : null
        };
    }

    public async Task<UserSummaryDto> ToggleUserStatusAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.CompanyProfile)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException($"Usuario {userId} no encontrado.");

        if (user.Role == UserRole.Admin)
            throw new InvalidOperationException("No se puede desactivar la cuenta de un administrador.");

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();

        return new UserSummaryDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Cedula = user.Cedula,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            EmailVerified = user.EmailVerified,
            CreatedAt = user.CreatedAt,
            ProfileName = user.Role == UserRole.Company ? user.CompanyProfile?.CompanyName : null
        };
    }

    public async Task DeleteUserAsync(int userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException($"Usuario {userId} no encontrado.");

        if (user.Role == UserRole.Admin)
            throw new InvalidOperationException("No se puede eliminar la cuenta de un administrador.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task<SystemStatsDto> GetStatsAsync()
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var totalCandidates = await _context.Users.CountAsync(u => u.Role == UserRole.Candidate);
        var totalCompanies = await _context.Users.CountAsync(u => u.Role == UserRole.Company);
        var totalOffers = await _context.JobOffers.CountAsync();
        var totalMatches = await _context.Matches.CountAsync();

        var activeTests = await _context.TestSubmissions
            .Where(s => s.Status == SubmissionStatus.Pending)
            .Select(s => s.TestId)
            .Distinct()
            .CountAsync();

        var pendingSubmissions = await _context.TestSubmissions
            .CountAsync(s => s.Status == SubmissionStatus.Pending);

        var offersByStatus = await _context.JobOffers
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var usersLast30Days = await _context.Users
            .CountAsync(u => u.CreatedAt >= thirtyDaysAgo);

        var offersLast30Days = await _context.JobOffers
            .CountAsync(o => o.CreatedAt >= thirtyDaysAgo);

        return new SystemStatsDto
        {
            TotalCandidates = totalCandidates,
            TotalCompanies = totalCompanies,
            TotalOffers = totalOffers,
            TotalMatches = totalMatches,
            ActiveTests = activeTests,
            PendingSubmissions = pendingSubmissions,
            OffersByStatus = offersByStatus.ToDictionary(x => x.Status, x => x.Count),
            UsersRegisteredLast30Days = usersLast30Days,
            OffersCreatedLast30Days = offersLast30Days
        };
    }
}
