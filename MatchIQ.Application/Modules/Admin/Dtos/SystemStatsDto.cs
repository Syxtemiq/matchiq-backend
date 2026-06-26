namespace MatchIQ.Application.Modules.Admin.Dtos;

public class SystemStatsDto
{
    public int TotalCandidates { get; set; }
    public int TotalCompanies { get; set; }
    public int TotalOffers { get; set; }
    public int TotalMatches { get; set; }
    public int ActiveTests { get; set; }             // tests con submissions en estado 'pending'
    public int PendingSubmissions { get; set; }      // submissions esperando evaluación
    public Dictionary<string, int> OffersByStatus { get; set; } = new();
    public int UsersRegisteredLast30Days { get; set; }
    public int OffersCreatedLast30Days { get; set; }
}