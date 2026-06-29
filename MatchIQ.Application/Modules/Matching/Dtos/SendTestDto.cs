using System.ComponentModel.DataAnnotations;

namespace MatchIQ.Application.Modules.Matching.Dtos;

public class SendTestDto
{
    [Required]
    public List<int> MatchIds { get; set; } = [];
}
