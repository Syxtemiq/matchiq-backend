using MatchIQ.Domain.Enums;

namespace MatchIQ.Domain.Entities;

// Oferta laboral creada por una empresa
// Al crearse dispara la generación del test y el matching automático
// Reglas de negocio:
//   - no se puede cancelar si ya está completada
//   - si hay candidatos en TestSent o TestCompleted al cancelar → warning primero
//   - solo se puede editar si está en estado Open
public class JobOffer
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Salary { get; set; }
    public Modality Modality { get; set; }
    public int? MinExperienceYears { get; set; }
    public EnglishLevel? RequiredEnglishLevel { get; set; }
    public int PositionsAvailable { get; set; } = 1;

    public int TierId { get; set; }
    public int? CandidatesToTest { get; set; }
    public int CandidatesTestedCount { get; set; }

    public int TestDeadlineDays { get; set; } = 3;
    public OfferStatus Status { get; set; } = OfferStatus.PendingPayment;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? TestSentAt { get; set; }

    public CompanyProfile CompanyProfile { get; set; } = null!;
    public PricingTier PricingTier { get; set; } = null!;
    public Payment? Payment { get; set; }
    public Test? Test { get; set; }
    public ICollection<OfferCategory> OfferCategories { get; set; } = new List<OfferCategory>();
    public ICollection<OfferSkill> OfferSkills { get; set; } = new List<OfferSkill>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
}
