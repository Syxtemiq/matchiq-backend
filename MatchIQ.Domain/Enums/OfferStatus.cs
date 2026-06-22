namespace MatchIQ.Domain.Enums;

// Ciclo de vida: PendingPayment → Open → TestSent → Completed
//                                   ↘ Cancelled / Expired (antes de TestSent)
public enum OfferStatus
{
    PendingPayment, // creada, esperando confirmación del pago vía webhook de Stripe
    Open,           // pagada, acumulando matches sin tope
    TestSent,       // el test ya se envió en bloque a los candidatos elegidos
    Completed,      // todas las submissions quedaron evaluadas o expiradas
    Cancelled,      // cancelada manualmente por la empresa
    Expired         // pasaron 3 meses sin enviar el test
}
