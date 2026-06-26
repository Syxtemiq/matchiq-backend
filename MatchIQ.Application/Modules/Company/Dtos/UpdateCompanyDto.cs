using System.ComponentModel.DataAnnotations;

namespace MatchIQ.Application.Modules.Company.Dtos;

public class UpdateCompanyDto
{
    [Required]
    public string CompanyName { get; set; } = string.Empty;
}
