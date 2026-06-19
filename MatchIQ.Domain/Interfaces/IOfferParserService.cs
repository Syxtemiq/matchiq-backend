namespace MatchIQ.Domain.Interfaces;

// Contrato para el parser de ofertas con IA
// Feature nueva: la empresa escribe una descripción libre y la IA extrae
// los campos estructurados para pre-llenar el formulario de la oferta
// El frontend muestra los resultados para que el admin confirme antes de guardar
public interface IOfferParserService
{
    // TODO: Task<ParsedOfferDto> ParseFromDescriptionAsync(string rawDescription)
    // Recibe: texto libre del admin ("busco backend python, 2 años exp, remoto, inglés B2")
    // Retorna: título sugerido, modalidad, salario, experiencia, nivel inglés,
    //          category_ids y skill_ids de la base de datos real
}
