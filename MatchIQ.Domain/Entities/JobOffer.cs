namespace MatchIQ.Domain.Entities;

// Oferta laboral creada por una empresa
// Al crearse dispara la generación del test y el matching automático
// Reglas de negocio:
//   - no se puede cancelar si ya está completada
//   - si hay candidatos en TestSent o TestCompleted al cancelar → warning primero
//   - solo se puede editar si está en estado Open
public class JobOffer
{
    // TODO: Id, CompanyId, Title, Description, Salary, Modality
    // TODO: MinExperienceYears, RequiredEnglishLevel, PositionsAvailable
    // TODO: Status (Open | InProcess | Completed | Cancelled), CreatedAt
    // TODO: navegación a CompanyProfile
    // TODO: colección de OfferCategory
    // TODO: colección de OfferSkill
    // TODO: colección de Match
    // TODO: relación 1-1 con Test
}
