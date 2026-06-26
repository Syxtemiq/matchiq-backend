using System.ComponentModel.DataAnnotations;

namespace MatchIQ.Application.Modules.Offers.Dtos;

public class ParseOfferDto
{
    [Required]
    public string RawDescription { get; set; } = string.Empty;
}
