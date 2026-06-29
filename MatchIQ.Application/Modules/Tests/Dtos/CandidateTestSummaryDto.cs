namespace MatchIQ.Application.Modules.Tests.Dtos;

public class CandidateTestSummaryDto
{
    public int TestId { get; set; }
    public int OfferId { get; set; }
    public string OfferTitle { get; set; } = string.Empty;
    public string TestTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? Deadline { get; set; }
    public int TimeLimitMinutes { get; set; }
    public decimal? Score { get; set; }
}
