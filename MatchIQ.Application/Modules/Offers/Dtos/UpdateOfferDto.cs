using System.ComponentModel.DataAnnotations;

namespace MatchIQ.Application.Modules.Offers.Dtos;

public class UpdateOfferDto
{
    [MinLength(1, ErrorMessage = "El título no puede estar vacío.")]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El salario no puede ser negativo.")]
    public decimal? Salary { get; set; }

    [RegularExpression(@"(?i)^(remote|onsite|hybrid)$", ErrorMessage = "La modalidad debe ser remote, onsite o hybrid.")]
    public string? Modality { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Los años de experiencia mínima no pueden ser negativos.")]
    public int? MinExperienceYears { get; set; }

    [RegularExpression(@"(?i)^(A1|A2|B1|B2|C1|C2)$", ErrorMessage = "El nivel de inglés debe ser A1, A2, B1, B2, C1 o C2.")]
    public string? RequiredEnglishLevel { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe haber al menos 1 posición disponible.")]
    public int? PositionsAvailable { get; set; }
}
