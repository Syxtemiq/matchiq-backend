namespace MatchIQ.Domain.Enums;

public enum PaymentStatus
{
    Pending,    // esperando confirmación de Stripe
    Succeeded,  // pago confirmado vía webhook
    Failed,     // el pago no se completó
    Refunded    // el pago fue reembolsado
}