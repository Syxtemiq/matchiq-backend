using System.ComponentModel.DataAnnotations;

namespace MatchIQ.Application.Modules.Offers.Dtos;

public class CreateOfferDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El salario no puede ser negativo.")]
    public decimal? Salary { get; set; }

    [Required]
    [RegularExpression(@"(?i)^(remote|onsite|hybrid)$", ErrorMessage = "La modalidad debe ser remote, onsite o hybrid.")]
    public string Modality { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Los años de experiencia mínima no pueden ser negativos.")]
    public int? MinExperienceYears { get; set; }

    [RegularExpression(@"(?i)^(A1|A2|B1|B2|C1|C2)$", ErrorMessage = "El nivel de inglés debe ser A1, A2, B1, B2, C1 o C2.")]
    public string? RequiredEnglishLevel { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe haber al menos 1 posición disponible.")]
    public int PositionsAvailable { get; set; } = 1;

    [Range(1, int.MaxValue, ErrorMessage = "El TierId debe ser un valor positivo.")]
    public int TierId { get; set; }

    [Required]
    [Range(1, 90, ErrorMessage = "El plazo para el test debe ser entre 1 y 90 días.")]
    public int TestDeadlineDays { get; set; }

    public List<int> CategoryIds { get; set; } = [];
    public List<int> SkillIds { get; set; } = [];
}
