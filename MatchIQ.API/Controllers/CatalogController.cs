namespace MatchIQ.API.Controllers;

// [ApiController]
// [Route("api/catalog")]
// Endpoints públicos (o con auth básica) para obtener categorías y skills
// Necesarios para llenar los formularios del frontend y para el OfferParserService
public class CatalogController // : ControllerBase
{
    // TODO: inyectar AppDbContext (directo, sin service — es solo lectura simple)

    // GET api/catalog/categories
    // TODO: GetCategoriesAsync()
    //       retorna todas las categorías

    // GET api/catalog/categories/{categoryId}/skills
    // TODO: GetSkillsByCategoryAsync(int categoryId)
    //       retorna los skills de una categoría (para mostrar según checkbox)
}
