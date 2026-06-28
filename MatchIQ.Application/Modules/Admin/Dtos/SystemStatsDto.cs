namespace MatchIQ.Application.Modules.Admin.Dtos;

public class SystemStatsDto
{
    // ── Usuarios ────────────────────────────────────────────────
    public int TotalCandidates { get; set; }
    public int TotalCompanies { get; set; }
    public int UsersRegisteredLast30Days { get; set; }

    // ── Ofertas ─────────────────────────────────────────────────
    public int TotalOffers { get; set; }
    public int OffersCreatedLast30Days { get; set; }
    public int OffersActive { get; set; }          // Open
    public int OffersCompleted { get; set; }       // Completed
    public int OffersCancelled { get; set; }       // Cancelled
    public int OffersExpired { get; set; }         // Expired
    public int OffersPendingPayment { get; set; }  // PendingPayment
    public Dictionary<string, int> OffersByStatus { get; set; } = new();

    // ── Matching ────────────────────────────────────────────────
    public int TotalMatches { get; set; }
    public int MatchesSelected { get; set; }       // contrataciones exitosas
    public int MatchesRejected { get; set; }
    public int MatchesTestSent { get; set; }
    public int MatchesTestCompleted { get; set; }

    // ── Tests ───────────────────────────────────────────────────
    public int ActiveTests { get; set; }           // tests con submissions pendientes
    public int PendingSubmissions { get; set; }    // esperando evaluación IA
    public int SubmissionsEvaluated { get; set; }
    public int SubmissionsExpired { get; set; }
    public decimal AverageTestScore { get; set; }

    // ── Ingresos ────────────────────────────────────────────────
    public decimal TotalRevenueCop { get; set; }
    public int PaymentsCompleted { get; set; }
    public int PaymentsPending { get; set; }

    // ── Tasas de conversión ─────────────────────────────────────
    public decimal TestCompletionRate { get; set; }  // % submissions evaluadas vs expiradas
    public decimal SelectionRate { get; set; }        // % seleccionados vs tests completados
}
