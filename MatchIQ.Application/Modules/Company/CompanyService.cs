using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Company.Dtos;
using MatchIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MatchIQ.Application.Modules.Company;

public class CompanyService
{
    private readonly IAppDbContext _context;

    public CompanyService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyProfileDto> GetProfileAsync(int userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        var profile = await _context.CompanyProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        return MapToDto(user, profile);
    }

    public async Task<CompanyProfileDto> UpdateProfileAsync(int userId, UpdateCompanyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CompanyName))
            throw new InvalidOperationException("El nombre de la empresa no puede estar vacío.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        var profile = await _context.CompanyProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null)
        {
            profile = new CompanyProfile { UserId = userId };
            _context.CompanyProfiles.Add(profile);
        }

        profile.CompanyName = dto.CompanyName.Trim();
        await _context.SaveChangesAsync();

        return MapToDto(user, profile);
    }

    private static CompanyProfileDto MapToDto(Domain.Entities.User user, CompanyProfile? profile) =>
        new()
        {
            UserId = user.Id,
            FullName = user.FullName ?? string.Empty,
            Email = user.Email,
            CompanyName = profile?.CompanyName,
            ProfileCompleted = profile?.CompanyName is not null,
            CreatedAt = profile?.CreatedAt ?? DateTime.UtcNow
        };
}
