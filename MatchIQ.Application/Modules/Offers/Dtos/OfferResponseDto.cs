using MatchIQ.Application.Modules.Catalog.Dtos;

namespace MatchIQ.Application.Modules.Offers.Dtos;

public class OfferResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Salary { get; set; }
    public string Modality { get; set; } = string.Empty;
    public int? MinExperienceYears { get; set; }
    public string? RequiredEnglishLevel { get; set; }
    public int PositionsAvailable { get; set; }
    public int TierId { get; set; }
    public string TierName { get; set; } = string.Empty;
    public decimal TierPriceCop { get; set; }
    public int? CandidatesToTest { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<CategoryDto> Categories { get; set; } = [];
    public List<SkillDto> Skills { get; set; } = [];
    public string? CheckoutUrl { get; set; }
}
