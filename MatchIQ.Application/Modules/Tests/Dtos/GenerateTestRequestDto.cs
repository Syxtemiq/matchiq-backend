using System.ComponentModel.DataAnnotations;

namespace MatchIQ.Application.Modules.Tests.Dtos;

public class GenerateTestRequestDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "El tiempo límite debe ser al menos 1 minuto.")]
    public int TimeLimitMinutes { get; set; }

    [RegularExpression(@"(?i)^(spanish|english)$", ErrorMessage = "El idioma del test debe ser spanish o english.")]
    public string? TestLanguage { get; set; }
}
