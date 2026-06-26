namespace MatchIQ.Application.Modules.Offers.Dtos;

public class CancelOfferResultDto
{
    public bool Cancelled { get; set; }
    public string? Warning { get; set; }
    public int CandidatesInProgress { get; set; }
}
