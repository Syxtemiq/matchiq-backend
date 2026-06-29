using System.ComponentModel.DataAnnotations;

namespace MatchIQ.Application.Modules.Candidate.Dtos;

public class SkillLevelDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El SkillId debe ser un valor positivo.")]
    public int SkillId { get; set; }

    [Range(1, 5, ErrorMessage = "El nivel del skill debe estar entre 1 y 5.")]
    public int Level { get; set; }
}
