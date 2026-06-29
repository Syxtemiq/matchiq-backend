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

        // ── Usuarios ────────────────────────────────────────────
        var totalCandidates       = await _context.Users.CountAsync(u => u.Role == UserRole.Candidate);
        var totalCompanies        = await _context.Users.CountAsync(u => u.Role == UserRole.Company);
        var usersLast30Days       = await _context.Users.CountAsync(u => u.CreatedAt >= thirtyDaysAgo);

        // ── Ofertas ─────────────────────────────────────────────
        var offersByStatus = await _context.JobOffers
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var offersMap       = offersByStatus.ToDictionary(x => x.Status, x => x.Count);
        var totalOffers     = offersByStatus.Sum(x => x.Count);
        var offersLast30Days = await _context.JobOffers.CountAsync(o => o.CreatedAt >= thirtyDaysAgo);

        // ── Matching ────────────────────────────────────────────
        var matchesByStage = await _context.Matches
            .GroupBy(m => m.Stage)
            .Select(g => new { Stage = g.Key, Count = g.Count() })
            .ToListAsync();

        var stageMap = matchesByStage.ToDictionary(x => x.Stage, x => x.Count);
        var totalMatches = matchesByStage.Sum(x => x.Count);

        // ── Tests ───────────────────────────────────────────────
        var submissionsByStatus = await _context.TestSubmissions
            .GroupBy(s => s.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var subMap = submissionsByStatus.ToDictionary(x => x.Status, x => x.Count);

        var activeTests = await _context.TestSubmissions
            .Where(s => s.Status == SubmissionStatus.Pending)
            .Select(s => s.TestId)
            .Distinct()
            .CountAsync();

        var pendingSubmissions = subMap.GetValueOrDefault(SubmissionStatus.Pending, 0);
        var evaluated          = subMap.GetValueOrDefault(SubmissionStatus.Evaluated, 0);
        var expired            = subMap.GetValueOrDefault(SubmissionStatus.Expired, 0);

        var avgScore = await _context.TestSubmissions
            .Where(s => s.Status == SubmissionStatus.Evaluated && s.Score != null)
            .AverageAsync(s => (decimal?)s.Score) ?? 0m;

        // ── Ingresos ────────────────────────────────────────────
        var totalRevenue       = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Succeeded)
            .SumAsync(p => (decimal?)p.AmountCop) ?? 0m;
        var paymentsCompleted  = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Succeeded);
        var paymentsPending    = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Pending);

        // ── Tasas ───────────────────────────────────────────────
        var totalFinishedSubs  = evaluated + expired;
        var testCompletionRate = totalFinishedSubs > 0
            ? Math.Round((decimal)evaluated / totalFinishedSubs * 100, 1)
            : 0m;

        var testCompleted      = stageMap.GetValueOrDefault(MatchStage.TestCompleted, 0);
        var selected           = stageMap.GetValueOrDefault(MatchStage.Selected, 0);
        var rejected           = stageMap.GetValueOrDefault(MatchStage.Rejected, 0);
        var selectionBase      = testCompleted + selected + rejected;
        var selectionRate      = selectionBase > 0
            ? Math.Round((decimal)selected / selectionBase * 100, 1)
            : 0m;

        return new SystemStatsDto
        {
            // Usuarios
            TotalCandidates          = totalCandidates,
            TotalCompanies           = totalCompanies,
            UsersRegisteredLast30Days = usersLast30Days,

            // Ofertas
            TotalOffers              = totalOffers,
            OffersCreatedLast30Days  = offersLast30Days,
            OffersActive             = offersMap.GetValueOrDefault(OfferStatus.Open, 0),
            OffersCompleted          = offersMap.GetValueOrDefault(OfferStatus.Completed, 0),
            OffersCancelled          = offersMap.GetValueOrDefault(OfferStatus.Cancelled, 0),
            OffersExpired            = offersMap.GetValueOrDefault(OfferStatus.Expired, 0),
            OffersPendingPayment     = offersMap.GetValueOrDefault(OfferStatus.PendingPayment, 0),
            OffersByStatus           = offersMap.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),

            // Matching
            TotalMatches             = totalMatches,
            MatchesSelected          = selected,
            MatchesRejected          = rejected,
            MatchesTestSent          = stageMap.GetValueOrDefault(MatchStage.TestSent, 0),
            MatchesTestCompleted     = testCompleted,

            // Tests
            ActiveTests              = activeTests,
            PendingSubmissions       = pendingSubmissions,
            SubmissionsEvaluated     = evaluated,
            SubmissionsExpired       = expired,
            AverageTestScore         = Math.Round(avgScore, 1),

            // Ingresos
            TotalRevenueCop          = totalRevenue,
            PaymentsCompleted        = paymentsCompleted,
            PaymentsPending          = paymentsPending,

            // Tasas
            TestCompletionRate       = testCompletionRate,
            SelectionRate            = selectionRate
        };
    }
}
