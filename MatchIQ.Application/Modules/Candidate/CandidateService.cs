using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Candidate.Dtos;
using MatchIQ.Application.Modules.Catalog.Dtos;
using MatchIQ.Domain.Entities;
using MatchIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MatchIQ.Application.Modules.Candidate;

public class CandidateService
{
    private readonly IAppDbContext _context;

    public CandidateService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CandidateProfileDto> GetProfileAsync(int userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        var profile = await _context.CandidateProfiles
            .Include(p => p.CandidateCategories)
                .ThenInclude(cc => cc.Category)
            .Include(p => p.CandidateSkills)
                .ThenInclude(cs => cs.Skill)
                    .ThenInclude(s => s.Category)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        return MapToDto(user, profile);
    }

    public async Task<CandidateProfileDto> UpdateProfileAsync(int userId, UpdateCandidateDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        if (dto.CategoryIds.Count > 0)
        {
            var validCategoryIds = await _context.Categories
                .Where(c => dto.CategoryIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            var invalidCats = dto.CategoryIds.Except(validCategoryIds).ToList();
            if (invalidCats.Count > 0)
                throw new InvalidOperationException($"Categorías no válidas: {string.Join(", ", invalidCats)}");
        }

        if (dto.Skills.Count > 0)
        {
            var skillIds = dto.Skills.Select(s => s.SkillId).ToList();
            var validSkillIds = await _context.Skills
                .Where(s => skillIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();

            var invalidSkills = skillIds.Except(validSkillIds).ToList();
            if (invalidSkills.Count > 0)
                throw new InvalidOperationException($"Skills no válidos: {string.Join(", ", invalidSkills)}");

            if (dto.Skills.Any(s => s.Level < 1 || s.Level > 5))
                throw new InvalidOperationException("El nivel de skill debe estar entre 1 y 5.");
        }

        // Upsert del perfil — INSERT primera vez activa el trigger de matching
        var profile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null)
        {
            profile = new CandidateProfile { UserId = userId };
            _context.CandidateProfiles.Add(profile);
        }

        if (dto.ExperienceYears.HasValue)
            profile.ExperienceYears = dto.ExperienceYears;

        if (dto.Seniority is not null)
        {
            if (!Enum.TryParse<Seniority>(dto.Seniority, ignoreCase: true, out var seniority))
                throw new InvalidOperationException($"El seniority '{dto.Seniority}' no es válido. Valores aceptados: Junior, Mid, Senior.");
            profile.Seniority = seniority;
        }

        if (dto.EnglishLevel is not null)
        {
            if (!Enum.TryParse<EnglishLevel>(dto.EnglishLevel, ignoreCase: true, out var englishLevel))
                throw new InvalidOperationException($"El nivel de inglés '{dto.EnglishLevel}' no es válido. Valores aceptados: A1, A2, B1, B2, C1, C2.");
            profile.EnglishLevel = englishLevel;
        }

        if (dto.GithubLink is not null)
            profile.GithubLink = dto.GithubLink;

        if (dto.LinkedinUrl is not null)
            profile.LinkedinUrl = dto.LinkedinUrl;

        if (dto.ProfilePhotoUrl is not null)
            profile.ProfilePhotoUrl = dto.ProfilePhotoUrl;

        await _context.SaveChangesAsync();

        // Reemplazar categorías
        var existingCategories = await _context.CandidateCategories
            .Where(cc => cc.CandidateId == profile.Id)
            .ToListAsync();
        _context.CandidateCategories.RemoveRange(existingCategories);

        foreach (var categoryId in dto.CategoryIds)
            _context.CandidateCategories.Add(new CandidateCategory
            {
                CandidateId = profile.Id,
                CategoryId = categoryId
            });

        // Reemplazar skills
        var existingSkills = await _context.CandidateSkills
            .Where(cs => cs.CandidateId == profile.Id)
            .ToListAsync();
        _context.CandidateSkills.RemoveRange(existingSkills);

        foreach (var s in dto.Skills)
            _context.CandidateSkills.Add(new CandidateSkill
            {
                CandidateId = profile.Id,
                SkillId = s.SkillId,
                Level = s.Level
            });

        await _context.SaveChangesAsync();

        var updated = await _context.CandidateProfiles
            .Include(p => p.CandidateCategories)
                .ThenInclude(cc => cc.Category)
            .Include(p => p.CandidateSkills)
                .ThenInclude(cs => cs.Skill)
                    .ThenInclude(s => s.Category)
            .FirstAsync(p => p.UserId == userId);

        return MapToDto(user, updated);
    }

    private static CandidateProfileDto MapToDto(Domain.Entities.User user, CandidateProfile? profile)
    {
        var profileCompleted = profile is not null
            && profile.ExperienceYears.HasValue
            && profile.Seniority.HasValue
            && profile.EnglishLevel.HasValue;

        return new CandidateProfileDto
        {
            UserId = user.Id,
            FullName = user.FullName ?? string.Empty,
            Email = user.Email,
            ExperienceYears = profile?.ExperienceYears,
            Seniority = profile?.Seniority?.ToString(),
            EnglishLevel = profile?.EnglishLevel?.ToString(),
            GithubLink = profile?.GithubLink,
            LinkedinUrl = profile?.LinkedinUrl,
            ProfilePhotoUrl = profile?.ProfilePhotoUrl,
            ProfileCompleted = profileCompleted,
            Categories = profile?.CandidateCategories
                .Select(cc => new CategoryDto { Id = cc.CategoryId, Name = cc.Category.Name })
                .ToList() ?? [],
            Skills = profile?.CandidateSkills
                .Select(cs => new CandidateSkillDto
                {
                    SkillId = cs.SkillId,
                    SkillName = cs.Skill.Name,
                    CategoryId = cs.Skill.CategoryId,
                    CategoryName = cs.Skill.Category.Name,
                    Level = cs.Level
                })
                .ToList() ?? []
        };
    }
}
