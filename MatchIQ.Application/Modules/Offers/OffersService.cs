using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Catalog.Dtos;
using MatchIQ.Application.Modules.Offers.Dtos;
using MatchIQ.Domain.Entities;
using MatchIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MatchIQ.Application.Modules.Offers;

public class OffersService
{
    private readonly IAppDbContext _context;
    private readonly IOfferParserService _parserService;

    public OffersService(IAppDbContext context, IOfferParserService parserService)
    {
        _context = context;
        _parserService = parserService;
    }

    public async Task<ParsedOfferResponseDto> ParseFromDescriptionAsync(string rawDescription)
    {
        if (string.IsNullOrWhiteSpace(rawDescription))
            throw new InvalidOperationException("La descripción no puede estar vacía.");

        return await _parserService.ParseFromDescriptionAsync(rawDescription);
    }

    public async Task<OfferResponseDto> CreateOfferAsync(int userId, CreateOfferDto dto)
    {
        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new InvalidOperationException("Debes completar tu perfil de empresa antes de crear una oferta.");

        if (company.CompanyName is null)
            throw new InvalidOperationException("Debes completar el nombre de la empresa antes de crear una oferta.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new InvalidOperationException("El título de la oferta no puede estar vacío.");

        if (dto.PositionsAvailable < 1)
            throw new InvalidOperationException("El número de posiciones debe ser al menos 1.");

        var tier = await _context.PricingTiers
            .FirstOrDefaultAsync(t => t.Id == dto.TierId && t.IsActive)
            ?? throw new KeyNotFoundException("Tier de precios no encontrado o inactivo.");

        if (!Enum.TryParse<Modality>(dto.Modality, ignoreCase: true, out var modality))
            throw new InvalidOperationException($"Modalidad inválida: '{dto.Modality}'. Valores válidos: Remote, Onsite, Hybrid.");

        EnglishLevel? requiredEnglish = null;
        if (dto.RequiredEnglishLevel is not null)
        {
            if (!Enum.TryParse<EnglishLevel>(dto.RequiredEnglishLevel, ignoreCase: true, out var eng))
                throw new InvalidOperationException($"Nivel de inglés inválido: '{dto.RequiredEnglishLevel}'. Valores válidos: A1, A2, B1, B2, C1, C2.");
            requiredEnglish = eng;
        }

        if (dto.CategoryIds.Count > 0)
        {
            var validCats = await _context.Categories
                .Where(c => dto.CategoryIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();
            var invalidCats = dto.CategoryIds.Except(validCats).ToList();
            if (invalidCats.Count > 0)
                throw new InvalidOperationException($"Categorías no válidas: {string.Join(", ", invalidCats)}");
        }

        if (dto.SkillIds.Count > 0)
        {
            var validSkills = await _context.Skills
                .Where(s => dto.SkillIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();
            var invalidSkills = dto.SkillIds.Except(validSkills).ToList();
            if (invalidSkills.Count > 0)
                throw new InvalidOperationException($"Skills no válidos: {string.Join(", ", invalidSkills)}");
        }

        var offer = new JobOffer
        {
            CompanyId = company.Id,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Salary = dto.Salary,
            Modality = modality,
            MinExperienceYears = dto.MinExperienceYears,
            RequiredEnglishLevel = requiredEnglish,
            PositionsAvailable = dto.PositionsAvailable,
            TierId = dto.TierId,
            CandidatesToTest = tier.MaxCandidates,
            TestDeadlineDays = dto.TestDeadlineDays,
            Status = OfferStatus.PendingPayment
        };

        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync();

        foreach (var catId in dto.CategoryIds)
            _context.OfferCategories.Add(new OfferCategory { OfferId = offer.Id, CategoryId = catId });

        foreach (var skillId in dto.SkillIds)
            _context.OfferSkills.Add(new OfferSkill { OfferId = offer.Id, SkillId = skillId });

        _context.Payments.Add(new Payment
        {
            OfferId = offer.Id,
            TierId = tier.Id,
            AmountCop = tier.PriceCop,
            Status = PaymentStatus.Pending
        });

        await _context.SaveChangesAsync();

        return MapToDto((await LoadFullOfferAsync(offer.Id))!);
    }

    public async Task<List<OfferResponseDto>> GetMyOffersAsync(int userId)
    {
        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        var offers = await _context.JobOffers
            .Include(o => o.OfferCategories).ThenInclude(oc => oc.Category)
            .Include(o => o.OfferSkills).ThenInclude(os => os.Skill)
            .Include(o => o.PricingTier)
            .Where(o => o.CompanyId == company.Id)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return offers.Select(MapToDto).ToList();
    }

    public async Task<OfferResponseDto> GetOfferByIdAsync(int userId, int offerId)
    {
        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        var offer = await LoadFullOfferAsync(offerId)
            ?? throw new KeyNotFoundException("Oferta no encontrada.");

        if (offer.CompanyId != company.Id)
            throw new UnauthorizedAccessException("No tienes acceso a esta oferta.");

        return MapToDto(offer);
    }

    public async Task<OfferResponseDto> UpdateOfferAsync(int userId, int offerId, UpdateOfferDto dto)
    {
        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        var offer = await LoadFullOfferAsync(offerId)
            ?? throw new KeyNotFoundException("Oferta no encontrada.");

        if (offer.CompanyId != company.Id)
            throw new UnauthorizedAccessException("No tienes acceso a esta oferta.");

        if (offer.Status != OfferStatus.PendingPayment)
            throw new InvalidOperationException("La oferta no puede ser modificada una vez que ha sido activada.");

        if (dto.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new InvalidOperationException("El título no puede estar vacío.");
            offer.Title = dto.Title.Trim();
        }

        if (dto.Description is not null)
            offer.Description = dto.Description.Trim();

        if (dto.Salary.HasValue)
            offer.Salary = dto.Salary;

        if (dto.Modality is not null)
        {
            if (!Enum.TryParse<Modality>(dto.Modality, ignoreCase: true, out var mod))
                throw new InvalidOperationException($"Modalidad inválida: '{dto.Modality}'.");
            offer.Modality = mod;
        }

        if (dto.MinExperienceYears.HasValue)
            offer.MinExperienceYears = dto.MinExperienceYears;

        if (dto.RequiredEnglishLevel is not null)
        {
            if (!Enum.TryParse<EnglishLevel>(dto.RequiredEnglishLevel, ignoreCase: true, out var eng))
                throw new InvalidOperationException($"Nivel de inglés inválido: '{dto.RequiredEnglishLevel}'.");
            offer.RequiredEnglishLevel = eng;
        }

        if (dto.PositionsAvailable.HasValue)
        {
            if (dto.PositionsAvailable.Value < 1)
                throw new InvalidOperationException("El número de posiciones debe ser al menos 1.");
            offer.PositionsAvailable = dto.PositionsAvailable.Value;
        }

        await _context.SaveChangesAsync();

        return MapToDto((await LoadFullOfferAsync(offerId))!);
    }

    public async Task<CancelOfferResultDto> CancelOfferAsync(int userId, int offerId)
    {
        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        var offer = await _context.JobOffers
            .Include(o => o.Matches)
            .FirstOrDefaultAsync(o => o.Id == offerId)
            ?? throw new KeyNotFoundException("Oferta no encontrada.");

        if (offer.CompanyId != company.Id)
            throw new UnauthorizedAccessException("No tienes acceso a esta oferta.");

        if (offer.Status == OfferStatus.Completed)
            throw new InvalidOperationException("No se puede cancelar una oferta completada.");

        if (offer.Status == OfferStatus.Cancelled)
            throw new InvalidOperationException("La oferta ya está cancelada.");

        var inProgress = offer.Matches
            .Count(m => m.Stage == MatchStage.TestSent || m.Stage == MatchStage.TestCompleted);

        if (inProgress > 0)
        {
            return new CancelOfferResultDto
            {
                Cancelled = false,
                Warning = $"Hay {inProgress} candidato(s) con un test en proceso. ¿Confirmas la cancelación?",
                CandidatesInProgress = inProgress
            };
        }

        offer.Status = OfferStatus.Cancelled;
        await _context.SaveChangesAsync();

        return new CancelOfferResultDto { Cancelled = true };
    }

    public async Task ForceCancelAsync(int userId, int offerId)
    {
        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        var offer = await _context.JobOffers
            .FirstOrDefaultAsync(o => o.Id == offerId)
            ?? throw new KeyNotFoundException("Oferta no encontrada.");

        if (offer.CompanyId != company.Id)
            throw new UnauthorizedAccessException("No tienes acceso a esta oferta.");

        if (offer.Status == OfferStatus.Completed)
            throw new InvalidOperationException("No se puede cancelar una oferta completada.");

        offer.Status = OfferStatus.Cancelled;
        await _context.SaveChangesAsync();
    }

    private async Task<JobOffer?> LoadFullOfferAsync(int offerId) =>
        await _context.JobOffers
            .Include(o => o.OfferCategories).ThenInclude(oc => oc.Category)
            .Include(o => o.OfferSkills).ThenInclude(os => os.Skill)
            .Include(o => o.PricingTier)
            .FirstOrDefaultAsync(o => o.Id == offerId);

    private static OfferResponseDto MapToDto(JobOffer offer) => new()
    {
        Id = offer.Id,
        Title = offer.Title,
        Description = offer.Description,
        Salary = offer.Salary,
        Modality = offer.Modality.ToString(),
        MinExperienceYears = offer.MinExperienceYears,
        RequiredEnglishLevel = offer.RequiredEnglishLevel?.ToString(),
        PositionsAvailable = offer.PositionsAvailable,
        TierId = offer.TierId,
        TierName = offer.PricingTier.Name,
        TierPriceCop = offer.PricingTier.PriceCop,
        CandidatesToTest = offer.CandidatesToTest,
        TestDeadlineDays = offer.TestDeadlineDays,
        Status = offer.Status.ToString(),
        CreatedAt = offer.CreatedAt,
        PaidAt = offer.PaidAt,
        ExpiresAt = offer.ExpiresAt,
        Categories = offer.OfferCategories
            .Select(oc => new CategoryDto { Id = oc.CategoryId, Name = oc.Category.Name })
            .ToList(),
        Skills = offer.OfferSkills
            .Select(os => new SkillDto { Id = os.SkillId, Name = os.Skill.Name, CategoryId = os.Skill.CategoryId })
            .ToList()
    };
}
