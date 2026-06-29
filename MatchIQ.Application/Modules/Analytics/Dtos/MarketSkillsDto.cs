namespace MatchIQ.Application.Modules.Analytics.Dtos;

public class MarketSkillsDto
{
    public List<SkillDemandDto> TopDemand { get; set; } = [];
    public List<SkillSupplyDto> TopSupply { get; set; } = [];
    public List<SkillComboDto> TopCombinations { get; set; } = [];
}

public class SkillDemandDto
{
    public string SkillName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int OfferCount { get; set; }
}

public class SkillSupplyDto
{
    public string SkillName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int CandidateCount { get; set; }
}

public class SkillComboDto
{
    public string SkillA { get; set; } = string.Empty;
    public string SkillB { get; set; } = string.Empty;
    public int OfferCount { get; set; }
}
