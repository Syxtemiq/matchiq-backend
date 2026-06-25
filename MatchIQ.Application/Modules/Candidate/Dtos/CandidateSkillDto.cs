namespace MatchIQ.Application.Modules.Candidate.Dtos;

public class CandidateSkillDto
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? Level { get; set; }
}
