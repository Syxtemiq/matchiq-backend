namespace MatchIQ.Application.Modules.Offers.Dtos;

public class ParsedOfferResponseDto
{
    public string? Title { get; set; }
    public string? Modality { get; set; }
    public decimal? Salary { get; set; }
    public int? MinExperienceYears { get; set; }
    public string? RequiredEnglishLevel { get; set; }
    public List<int> SuggestedCategoryIds { get; set; } = [];
    public List<int> SuggestedSkillIds { get; set; } = [];
    public string? ConfidenceNote { get; set; }
}
