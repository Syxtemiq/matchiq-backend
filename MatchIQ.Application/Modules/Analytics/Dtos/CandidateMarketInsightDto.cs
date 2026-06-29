namespace MatchIQ.Application.Modules.Analytics.Dtos;

public class CandidateMarketInsightDto
{
    public List<DemandSkillWithPresenceDto> TopDemand { get; set; } = [];
    public List<SkillSupplyDto> TopSupply { get; set; } = [];
    public List<ComboWithPresenceDto> TopCombinations { get; set; } = [];
    public List<string> SkillsInDemand { get; set; } = [];
    public List<string> SkillGaps { get; set; } = [];
}

public class DemandSkillWithPresenceDto
{
    public string SkillName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int OfferCount { get; set; }
    public bool CandidateHasSkill { get; set; }
    public int? CandidateLevel { get; set; }
}

public class ComboWithPresenceDto
{
    public string SkillA { get; set; } = string.Empty;
    public string SkillB { get; set; } = string.Empty;
    public int OfferCount { get; set; }
    public bool CandidateHasA { get; set; }
    public bool CandidateHasB { get; set; }
    public bool CandidateHasBoth { get; set; }
}
