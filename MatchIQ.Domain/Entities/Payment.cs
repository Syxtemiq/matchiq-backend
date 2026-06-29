using MatchIQ.Domain.Enums;

namespace MatchIQ.Domain.Entities;

// Un pago por oferta (pago único, no suscripción)
// El estado se actualiza vía webhook de Stripe, nunca desde el frontend directamente
public class Payment
{
    public int Id { get; set; }
    public int OfferId { get; set; }
    public int TierId { get; set; }
    public string? PaymentTransactionId { get; set; }
    public string? PaymentCheckoutId { get; set; }
    public decimal AmountCop { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public JobOffer JobOffer { get; set; } = null!;
    public PricingTier PricingTier { get; set; } = null!;
}
