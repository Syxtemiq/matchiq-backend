namespace MatchIQ.Application.Modules.Offers.Dtos;

public class CreateOfferDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Salary { get; set; }
    public string Modality { get; set; } = string.Empty;
    public int? MinExperienceYears { get; set; }
    public string? RequiredEnglishLevel { get; set; }
    public int PositionsAvailable { get; set; } = 1;
    public int TierId { get; set; }
    public List<int> CategoryIds { get; set; } = [];
    public List<int> SkillIds { get; set; } = [];
}
