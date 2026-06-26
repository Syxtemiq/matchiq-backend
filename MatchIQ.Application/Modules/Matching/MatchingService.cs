using System.Text.Json;
using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Common.Interfaces.Repositories;
using MatchIQ.Application.Common.Dtos;
using MatchIQ.Application.Modules.Matching.Dtos;
using MatchIQ.Domain.Entities;
using MatchIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MatchIQ.Application.Modules.Matching;

public class MatchingService
{
    private readonly IAppDbContext _context;
    private readonly IMatchRepository _matchRepository;
    private readonly IAIService _aiService;
    private readonly IEmailService _emailService;
    private readonly string _frontendUrl;

    public MatchingService(
        IAppDbContext context,
        IMatchRepository matchRepository,
        IAIService aiService,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _context = context;
        _matchRepository = matchRepository;
        _aiService = aiService;
        _emailService = emailService;
        _frontendUrl = configuration["App:FrontendUrl"]?.TrimEnd('/') ?? "";
    }

    // Corre la función SQL de matching y enriquece el top 3 con insight de IA.
    // Llamado internamente al activar una oferta (webhook de Stripe) o manualmente.
    public async Task<List<MatchResultDto>> RunMatchingAsync(int offerId, int userId)
    {
        var offer = await _context.JobOffers
            .Include(o => o.OfferSkills)
            .Include(o => o.OfferCategories)
            .FirstOrDefaultAsync(o => o.Id == offerId)
            ?? throw new KeyNotFoundException("Oferta no encontrada.");

        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        if (offer.CompanyId != company.Id)
            throw new UnauthorizedAccessException("No tienes acceso a esta oferta.");

        if (offer.Status != OfferStatus.Open)
            throw new InvalidOperationException("El matching solo se puede ejecutar sobre ofertas en estado Open.");

        var matches = (await _matchRepository.RunMatchingAsync(offerId)).ToList();

        // Enriquecimiento IA: top 3 que aún no tienen adjusted_score
        var top3 = matches
            .Where(m => m.AdjustedScore is null)
            .Take(3)
            .ToList();

        foreach (var match in top3)
        {
            try
            {
                var insight = await _aiService.EvaluateCandidateAsync(offer, match);
                var raw = 0.9m * (match.MatchPercentage ?? 0) + 0.1m * insight.FitScore * 100;

                var dbMatch = await _context.Matches.FindAsync(match.Id);
                if (dbMatch is null) continue;

                dbMatch.AdjustedScore = Math.Min(100, raw);
                dbMatch.AiFeedback = JsonSerializer.Serialize(insight);
                dbMatch.UpdatedAt = DateTime.UtcNow;
            }
            catch
            {
                // IA es best-effort: seguimos sin insights si falla
            }
        }

        await _context.SaveChangesAsync();

        return await LoadMatchDtosAsync(offerId);
    }

    public async Task<List<MatchResultDto>> GetMatchesByOfferAsync(int userId, int offerId)
    {
        await VerifyOwnershipAsync(userId, offerId);
        return await LoadMatchDtosAsync(offerId);
    }

    public async Task SendTestsAsync(int userId, SendTestDto dto)
    {
        if (dto.MatchIds.Count == 0)
            throw new InvalidOperationException("Debes seleccionar al menos un candidato.");

        // Carga todos los matches indicados en una sola consulta
        var matches = await _context.Matches
            .Include(m => m.CandidateProfile).ThenInclude(cp => cp.User)
            .Include(m => m.JobOffer).ThenInclude(o => o.Test)
            .Where(m => dto.MatchIds.Contains(m.Id))
            .ToListAsync();

        if (matches.Count != dto.MatchIds.Count)
            throw new KeyNotFoundException("Uno o más matches no fueron encontrados.");

        // Verificar que todos pertenezcan a la misma oferta de esta empresa
        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        var offerIds = matches.Select(m => m.OfferId).Distinct().ToList();
        if (offerIds.Count > 1)
            throw new InvalidOperationException("Todos los matches deben pertenecer a la misma oferta.");

        var offer = matches[0].JobOffer;
        if (offer.CompanyId != company.Id)
            throw new UnauthorizedAccessException("No tienes acceso a esta oferta.");

        if (offer.Test is null)
            throw new InvalidOperationException("La oferta aún no tiene un test generado. Espera a que la IA genere el test.");

        // Validar estados
        var invalid = matches.Where(m => m.Stage != MatchStage.Matched).ToList();
        if (invalid.Count > 0)
            throw new InvalidOperationException($"Solo se puede enviar el test a candidatos en estado Matched. " +
                $"Los siguientes ya tienen otro estado: {string.Join(", ", invalid.Select(m => m.Id))}");

        // Verificar que no excedan el límite del tier
        var alreadySent = await _context.Matches
            .CountAsync(m => m.OfferId == offer.Id && m.Stage != MatchStage.Matched && m.Stage != MatchStage.Rejected);

        if (offer.CandidatesToTest.HasValue && alreadySent + matches.Count > offer.CandidatesToTest.Value)
            throw new InvalidOperationException(
                $"El tier de esta oferta permite máximo {offer.CandidatesToTest} candidatos con test. " +
                $"Ya tienes {alreadySent} y estás intentando agregar {matches.Count} más.");

        var deadline = DateTime.UtcNow.AddHours(72);

        foreach (var match in matches)
        {
            var existingSubmission = await _context.TestSubmissions
                .AnyAsync(s => s.TestId == offer.Test.Id && s.CandidateId == match.CandidateId);

            if (!existingSubmission)
            {
                _context.TestSubmissions.Add(new TestSubmission
                {
                    TestId = offer.Test.Id,
                    CandidateId = match.CandidateId,
                    Status = SubmissionStatus.Pending,
                    Deadline = deadline
                });
            }

            match.Stage = MatchStage.TestSent;
            match.UpdatedAt = DateTime.UtcNow;
        }

        // Activar el estado TestSent en la oferta (solo la primera vez)
        if (offer.Status == OfferStatus.Open)
        {
            offer.Status = OfferStatus.TestSent;
            offer.TestSentAt = DateTime.UtcNow;
        }

        // Guardar primero — si el correo falla, la submission ya existe en BD
        await _context.SaveChangesAsync();

        // Enviar correos con link directo al test (best-effort: un fallo no revierte la operación)
        var loginUrl = $"{_frontendUrl}/login?next=test/{offer.Id}";
        foreach (var match in matches)
        {
            try
            {
                await _emailService.SendTestInvitationAsync(
                    match.CandidateProfile.User.Email,
                    offer.Title,
                    offer.Test.TimeLimitMinutes,
                    loginUrl);
            }
            catch
            {
                // No propagar: la submission ya está creada, el candidato puede ver el test al ingresar
            }
        }
    }

    public async Task<List<MatchResultDto>> ReevaluateAsync(int offerId, int userId)
    {
        var offer = await _context.JobOffers
            .Include(o => o.OfferSkills).ThenInclude(os => os.Skill)
            .Include(o => o.OfferCategories).ThenInclude(oc => oc.Category)
            .FirstOrDefaultAsync(o => o.Id == offerId)
            ?? throw new KeyNotFoundException("Oferta no encontrada.");

        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        if (offer.CompanyId != company.Id)
            throw new UnauthorizedAccessException("No tienes acceso a esta oferta.");

        if (offer.Status != OfferStatus.Open)
            throw new InvalidOperationException("Solo se puede reevaluar una oferta en estado Open.");

        // Recalcula porcentajes de TODOS los candidatos (incluyendo los que ya tenían match)
        await _matchRepository.ReevaluateAllAsync(offerId);

        // Recargar todos los matches con sus datos
        var allMatches = await _context.Matches
            .Include(m => m.CandidateProfile).ThenInclude(cp => cp.User)
            .Include(m => m.CandidateProfile).ThenInclude(cp => cp.CandidateSkills).ThenInclude(cs => cs.Skill)
            .Where(m => m.OfferId == offerId)
            .ToListAsync();

        // Los que ya tienen feedback de IA: recalcular adjusted_score con el nuevo
        // match_percentage sin volver a llamar a la IA (el fit cualitativo no cambia)
        foreach (var match in allMatches.Where(m => m.AiFeedback is not null))
        {
            try
            {
                var insight = JsonSerializer.Deserialize<CandidateInsightDto>(match.AiFeedback!);
                if (insight is not null)
                {
                    match.AdjustedScore = Math.Min(100,
                        0.9m * (match.MatchPercentage ?? 0) + 0.1m * insight.FitScore * 100);
                    match.UpdatedAt = DateTime.UtcNow;
                }
            }
            catch { }
        }

        // Top 3 sin feedback de IA: evaluar con IA por primera vez
        var top3New = allMatches
            .Where(m => m.AiFeedback is null)
            .OrderByDescending(m => m.MatchPercentage)
            .Take(3)
            .ToList();

        foreach (var match in top3New)
        {
            try
            {
                var insight = await _aiService.EvaluateCandidateAsync(offer, match);
                match.AdjustedScore = Math.Min(100,
                    0.9m * (match.MatchPercentage ?? 0) + 0.1m * insight.FitScore * 100);
                match.AiFeedback = JsonSerializer.Serialize(insight);
                match.UpdatedAt = DateTime.UtcNow;
            }
            catch { }
        }

        await _context.SaveChangesAsync();
        return await LoadMatchDtosAsync(offerId);
    }

    public async Task RejectCandidateAsync(int userId, int matchId)
    {
        var match = await _context.Matches
            .Include(m => m.CandidateProfile).ThenInclude(cp => cp.User)
            .Include(m => m.JobOffer)
            .FirstOrDefaultAsync(m => m.Id == matchId)
            ?? throw new KeyNotFoundException("Match no encontrado.");

        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        if (match.JobOffer.CompanyId != company.Id)
            throw new UnauthorizedAccessException("No tienes acceso a este match.");

        if (match.Stage == MatchStage.Selected)
            throw new InvalidOperationException("No se puede rechazar un candidato que ya fue seleccionado.");

        if (match.Stage == MatchStage.Rejected)
            throw new InvalidOperationException("El candidato ya fue rechazado.");

        // Se permite rechazar desde: Matched, TestSent, TestCompleted
        match.Stage = MatchStage.Rejected;
        match.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        try
        {
            await _emailService.SendCandidateRejectedAsync(
                match.CandidateProfile.User.Email,
                match.CandidateProfile.User.FullName ?? match.CandidateProfile.User.Email,
                match.JobOffer.Title,
                _frontendUrl);
        }
        catch { /* best-effort: el rechazo ya se guardó */ }
    }

    public async Task<MatchResultDto> SelectCandidateAsync(int userId, int matchId)
    {
        var match = await _context.Matches
            .Include(m => m.CandidateProfile).ThenInclude(cp => cp.User)
            .Include(m => m.CandidateProfile).ThenInclude(cp => cp.CandidateSkills).ThenInclude(cs => cs.Skill)
            .Include(m => m.JobOffer).ThenInclude(o => o.OfferSkills)
            .Include(m => m.JobOffer).ThenInclude(o => o.CompanyProfile)
            .FirstOrDefaultAsync(m => m.Id == matchId)
            ?? throw new KeyNotFoundException("Match no encontrado.");

        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        if (match.JobOffer.CompanyId != company.Id)
            throw new UnauthorizedAccessException("No tienes acceso a este match.");

        if (match.Stage != MatchStage.TestCompleted)
            throw new InvalidOperationException("Solo puedes seleccionar candidatos que hayan completado el test.");

        match.Stage = MatchStage.Selected;
        match.UpdatedAt = DateTime.UtcNow;

        // Si se llenaron todas las posiciones → cierra la oferta
        var selectedCount = await _context.Matches
            .CountAsync(m => m.OfferId == match.OfferId && m.Stage == MatchStage.Selected);

        // +1 porque aún no se guardó el cambio actual
        if (selectedCount + 1 >= match.JobOffer.PositionsAvailable)
        {
            var offer = await _context.JobOffers.FindAsync(match.OfferId);
            if (offer is not null)
                offer.Status = OfferStatus.Completed;
        }

        await _context.SaveChangesAsync();

        try
        {
            await _emailService.SendCandidateSelectedAsync(
                match.CandidateProfile.User.Email,
                match.CandidateProfile.User.FullName ?? match.CandidateProfile.User.Email,
                match.JobOffer.Title,
                match.JobOffer.CompanyProfile.CompanyName ?? "la empresa",
                _frontendUrl);
        }
        catch { /* best-effort: la selección ya se guardó */ }

        var offerSkillIds = match.JobOffer.OfferSkills.Select(os => os.SkillId).ToHashSet();
        return MapToDto(match, offerSkillIds);
    }

    // ── Helpers privados ─────────────────────────────────────────────────────────

    private async Task VerifyOwnershipAsync(int userId, int offerId)
    {
        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        var offer = await _context.JobOffers
            .FirstOrDefaultAsync(o => o.Id == offerId)
            ?? throw new KeyNotFoundException("Oferta no encontrada.");

        if (offer.CompanyId != company.Id)
            throw new UnauthorizedAccessException("No tienes acceso a esta oferta.");
    }

    private async Task<List<MatchResultDto>> LoadMatchDtosAsync(int offerId)
    {
        var offerSkillIds = await _context.OfferSkills
            .Where(os => os.OfferId == offerId)
            .Select(os => os.SkillId)
            .ToHashSetAsync();

        var matches = await _context.Matches
            .Include(m => m.CandidateProfile).ThenInclude(cp => cp.User)
            .Include(m => m.CandidateProfile).ThenInclude(cp => cp.CandidateSkills).ThenInclude(cs => cs.Skill)
            .Where(m => m.OfferId == offerId)
            .OrderByDescending(m => m.AdjustedScore ?? m.MatchPercentage)
            .ToListAsync();

        return matches.Select(m => MapToDto(m, offerSkillIds)).ToList();
    }

    private static MatchResultDto MapToDto(Match match, HashSet<int> offerSkillIds)
    {
        CandidateInsightDto? insight = null;
        if (match.AiFeedback is not null)
        {
            try { insight = JsonSerializer.Deserialize<CandidateInsightDto>(match.AiFeedback); }
            catch { /* ignore parse errors */ }
        }

        var matchedSkills = match.CandidateProfile.CandidateSkills
            .Where(cs => offerSkillIds.Contains(cs.SkillId))
            .Select(cs => cs.Skill.Name)
            .ToList();

        return new MatchResultDto
        {
            MatchId = match.Id,
            CandidateId = match.CandidateId,
            FullName = match.CandidateProfile.User.FullName ?? string.Empty,
            Email = match.CandidateProfile.User.Email,
            ExperienceYears = match.CandidateProfile.ExperienceYears,
            EnglishLevel = match.CandidateProfile.EnglishLevel?.ToString(),
            MatchPercentage = match.MatchPercentage,
            AdjustedScore = match.AdjustedScore,
            Stage = match.Stage.ToString(),
            AiInsight = insight?.Insight,
            AiStrengths = insight?.Strengths ?? [],
            AiOpportunities = insight?.Opportunities ?? [],
            AiRecommendation = insight?.Recommendation,
            MatchedSkills = matchedSkills,
            CreatedAt = match.CreatedAt
        };
    }
}
