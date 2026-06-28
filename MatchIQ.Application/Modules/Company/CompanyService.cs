using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Company.Dtos;
using MatchIQ.Domain.Entities;
using MatchIQ.Domain.Enums;
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

    public async Task<CompanyDashboardDto> GetDashboardAsync(int userId)
    {
        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        var offerIds = await _context.JobOffers
            .Where(o => o.CompanyId == company.Id)
            .Select(o => o.Id)
            .ToListAsync();

        var offers = await _context.JobOffers
            .Where(o => o.CompanyId == company.Id)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total         = g.Count(),
                Open          = g.Count(o => o.Status == OfferStatus.Open),
                TestSent      = g.Count(o => o.Status == OfferStatus.TestSent),
                Completed     = g.Count(o => o.Status == OfferStatus.Completed),
                Cancelled     = g.Count(o => o.Status == OfferStatus.Cancelled),
                Expired       = g.Count(o => o.Status == OfferStatus.Expired),
                PendingPayment = g.Count(o => o.Status == OfferStatus.PendingPayment),
            })
            .FirstOrDefaultAsync();

        var matches = await _context.Matches
            .Where(m => offerIds.Contains(m.OfferId))
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total         = g.Count(),
                TestSent      = g.Count(m => m.Stage == MatchStage.TestSent),
                TestCompleted = g.Count(m => m.Stage == MatchStage.TestCompleted),
                Selected      = g.Count(m => m.Stage == MatchStage.Selected),
                Rejected      = g.Count(m => m.Stage == MatchStage.Rejected),
            })
            .FirstOrDefaultAsync();

        var testIds = await _context.Tests
            .Where(t => offerIds.Contains(t.OfferId))
            .Select(t => t.Id)
            .ToListAsync();

        var submissions = await _context.TestSubmissions
            .Where(s => testIds.Contains(s.TestId))
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total     = g.Count(),
                Evaluated = g.Count(s => s.Status == SubmissionStatus.Evaluated),
                Expired   = g.Count(s => s.Status == SubmissionStatus.Expired),
                AvgScore  = g.Where(s => s.Score != null).Average(s => (double?)s.Score),
            })
            .FirstOrDefaultAsync();

        int matchTestSent      = matches?.TestSent ?? 0;
        int matchSelected      = matches?.Selected ?? 0;
        int submissionsTotal   = submissions?.Total ?? 0;
        int submissionsEval    = submissions?.Evaluated ?? 0;

        return new CompanyDashboardDto
        {
            Offers = new OfferStatsDto
            {
                Total          = offers?.Total ?? 0,
                Open           = offers?.Open ?? 0,
                TestSent       = offers?.TestSent ?? 0,
                Completed      = offers?.Completed ?? 0,
                Cancelled      = offers?.Cancelled ?? 0,
                Expired        = offers?.Expired ?? 0,
                PendingPayment = offers?.PendingPayment ?? 0,
            },
            Matches = new MatchStatsDto
            {
                Total         = matches?.Total ?? 0,
                TestSent      = matchTestSent,
                TestCompleted = matches?.TestCompleted ?? 0,
                Selected      = matchSelected,
                Rejected      = matches?.Rejected ?? 0,
                SelectionRate = matchTestSent > 0
                    ? Math.Round((double)matchSelected / matchTestSent * 100, 1)
                    : 0,
            },
            Tests = new TestStatsDto
            {
                Sent           = matchTestSent,
                Completed      = submissionsTotal,
                Evaluated      = submissionsEval,
                Expired        = submissions?.Expired ?? 0,
                CompletionRate = matchTestSent > 0
                    ? Math.Round((double)submissionsTotal / matchTestSent * 100, 1)
                    : 0,
                AverageScore   = submissions?.AvgScore.HasValue == true
                    ? Math.Round(submissions.AvgScore.Value, 1)
                    : null,
            }
        };
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
