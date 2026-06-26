using MatchIQ.API.Common;
using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Catalog.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchIQ.API.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private readonly IAppDbContext _context;

    public CatalogController(IAppDbContext context)
    {
        _context = context;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name })
            .ToListAsync();

        return Ok(ApiResponse.Ok(categories));
    }

    [HttpGet("categories/{categoryId:int}/skills")]
    public async Task<IActionResult> GetSkillsByCategory(int categoryId)
    {
        var exists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
        if (!exists)
            throw new KeyNotFoundException($"Categoría {categoryId} no encontrada.");

        var skills = await _context.Skills
            .Where(s => s.CategoryId == categoryId)
            .OrderBy(s => s.Name)
            .Select(s => new SkillDto { Id = s.Id, Name = s.Name, CategoryId = s.CategoryId })
            .ToListAsync();

        return Ok(ApiResponse.Ok(skills));
    }
}
