namespace MatchIQ.Application.Modules.Offers.Dtos;

public class UpdateOfferDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? Salary { get; set; }
    public string? Modality { get; set; }
    public int? MinExperienceYears { get; set; }
    public string? RequiredEnglishLevel { get; set; }
    public int? PositionsAvailable { get; set; }
}
