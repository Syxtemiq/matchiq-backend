using System.ComponentModel.DataAnnotations;

namespace MatchIQ.Application.Modules.Tests.Dtos;

public class GenerateTestRequestDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "El tiempo límite debe ser al menos 1 minuto.")]
    public int TimeLimitMinutes { get; set; }
}
