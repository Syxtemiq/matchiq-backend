namespace MatchIQ.Application.Modules.Company.Dtos;

public class CompanyDashboardDto
{
    public OfferStatsDto Offers { get; set; } = new();
    public MatchStatsDto Matches { get; set; } = new();
    public TestStatsDto Tests { get; set; } = new();
}

public class OfferStatsDto
{
    public int Total { get; set; }
    public int Open { get; set; }
    public int TestSent { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public int Expired { get; set; }
    public int PendingPayment { get; set; }
}

public class MatchStatsDto
{
    public int Total { get; set; }
    public int TestSent { get; set; }
    public int TestCompleted { get; set; }
    public int Selected { get; set; }
    public int Rejected { get; set; }
    public double SelectionRate { get; set; }
}

public class TestStatsDto
{
    public int Sent { get; set; }
    public int Completed { get; set; }
    public int Evaluated { get; set; }
    public int Expired { get; set; }
    public double CompletionRate { get; set; }
    public double? AverageScore { get; set; }
}
