using ClosedXML.Excel;
using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MatchIQ.Infrastructure.Reports;

public class ReportService : IReportService
{
    private readonly IAppDbContext _context;

    public ReportService(IAppDbContext context)
    {
        _context = context;
    }

    // ── Empresa ───────────────────────────────────────────────────────────────

    public async Task<byte[]> GenerateCompanyReportAsync(int companyUserId)
    {
        var company = await _context.CompanyProfiles
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == companyUserId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        var offers = await _context.JobOffers
            .Include(o => o.PricingTier)
            .Include(o => o.Matches)
                .ThenInclude(m => m.CandidateProfile)
                    .ThenInclude(cp => cp.User)
            .Include(o => o.Test)
                .ThenInclude(t => t!.TestSubmissions)
            .Where(o => o.CompanyId == company.Id)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        using var workbook = new XLWorkbook();

        // ── Hoja 1: Mis Ofertas ───────────────────────────────────────────────
        var wsOffers = workbook.Worksheets.Add("Mis Ofertas");

        var offerHeaders = new[]
        {
            "Título", "Estado", "Modalidad", "Salario (COP)", "Posiciones",
            "Tier", "Candidatos a testear", "Test enviado el", "Creada el",
            "Total matches", "Tests enviados", "Tests completados",
            "Evaluados por IA", "Seleccionados", "Puntaje promedio"
        };
        WriteHeaders(wsOffers, offerHeaders);

        int row = 2;
        foreach (var offer in offers)
        {
            var matches       = offer.Matches.ToList();
            var testSent      = matches.Count(m => m.Stage >= MatchStage.TestSent);
            var testCompleted = matches.Count(m => m.Stage >= MatchStage.TestCompleted);
            var selected      = matches.Count(m => m.Stage == MatchStage.Selected);

            var submissions   = offer.Test?.TestSubmissions.ToList() ?? [];
            var evaluated     = submissions.Count(s => s.Status == SubmissionStatus.Evaluated);
            var avgScore      = submissions.Any(s => s.Score.HasValue)
                ? Math.Round(submissions.Where(s => s.Score.HasValue).Average(s => (double)s.Score!.Value), 1)
                : (double?)null;

            wsOffers.Cell(row, 1).Value  = offer.Title;
            wsOffers.Cell(row, 2).Value  = offer.Status.ToString();
            wsOffers.Cell(row, 3).Value  = offer.Modality.ToString();
            wsOffers.Cell(row, 4).Value  = offer.Salary.HasValue ? (double)offer.Salary.Value : (XLCellValue)"—";
            wsOffers.Cell(row, 5).Value  = offer.PositionsAvailable;
            wsOffers.Cell(row, 6).Value  = offer.PricingTier.Name;
            wsOffers.Cell(row, 7).Value  = offer.CandidatesToTest ?? 0;
            wsOffers.Cell(row, 8).Value  = offer.TestSentAt.HasValue ? offer.TestSentAt.Value.ToString("dd/MM/yyyy") : "—";
            wsOffers.Cell(row, 9).Value  = offer.CreatedAt.ToString("dd/MM/yyyy");
            wsOffers.Cell(row, 10).Value = matches.Count;
            wsOffers.Cell(row, 11).Value = testSent;
            wsOffers.Cell(row, 12).Value = testCompleted;
            wsOffers.Cell(row, 13).Value = evaluated;
            wsOffers.Cell(row, 14).Value = selected;
            wsOffers.Cell(row, 15).Value = avgScore.HasValue ? avgScore.Value : (XLCellValue)"—";
            row++;
        }

        FormatSheet(wsOffers, offerHeaders.Length, row - 1);

        // ── Hoja 2: Pipeline de Candidatos ───────────────────────────────────
        var wsPipeline = workbook.Worksheets.Add("Pipeline de Candidatos");

        var pipelineHeaders = new[]
        {
            "Candidato", "Email", "Oferta", "Etapa",
            "Puntaje IA", "Feedback global IA", "Test enviado el", "IA evaluó el"
        };
        WriteHeaders(wsPipeline, pipelineHeaders);

        row = 2;
        foreach (var offer in offers)
        {
            foreach (var match in offer.Matches.OrderByDescending(m => m.Stage))
            {
                var candidate   = match.CandidateProfile?.User;
                var submission  = offer.Test?.TestSubmissions
                    .FirstOrDefault(s => s.CandidateId == match.CandidateId);

                string? globalFeedback = null;
                if (submission?.Feedback is not null)
                {
                    try
                    {
                        var eval = System.Text.Json.JsonSerializer
                            .Deserialize<FeedbackWrapper>(submission.Feedback);
                        globalFeedback = eval?.Feedback;
                    }
                    catch { }
                }

                wsPipeline.Cell(row, 1).Value = candidate?.FullName ?? "—";
                wsPipeline.Cell(row, 2).Value = candidate?.Email ?? "—";
                wsPipeline.Cell(row, 3).Value = offer.Title;
                wsPipeline.Cell(row, 4).Value = match.Stage.ToString();
                wsPipeline.Cell(row, 5).Value = submission?.Score.HasValue == true
                    ? (XLCellValue)(double)submission.Score.Value : "—";
                wsPipeline.Cell(row, 6).Value = globalFeedback ?? "—";
                wsPipeline.Cell(row, 7).Value = submission?.SubmittedAt.HasValue == true
                    ? submission.SubmittedAt.Value.ToString("dd/MM/yyyy HH:mm") : "—";
                wsPipeline.Cell(row, 8).Value = submission?.AiEvaluatedAt.HasValue == true
                    ? submission.AiEvaluatedAt.Value.ToString("dd/MM/yyyy HH:mm") : "—";
                row++;
            }
        }

        FormatSheet(wsPipeline, pipelineHeaders.Length, row - 1);

        return ToBytes(workbook);
    }

    // ── Admin ─────────────────────────────────────────────────────────────────

    public async Task<byte[]> GenerateAdminReportAsync()
    {
        using var workbook = new XLWorkbook();

        await BuildPlatformSummarySheet(workbook);
        await BuildCompaniesSheet(workbook);
        await BuildPaymentsSheet(workbook);

        return ToBytes(workbook);
    }

    private async Task BuildPlatformSummarySheet(XLWorkbook workbook)
    {
        var ws = workbook.Worksheets.Add("Resumen Plataforma");

        ws.Cell(1, 1).Value = "Indicador";
        ws.Cell(1, 2).Value = "Valor";
        StyleHeaderRow(ws, 1, 2);

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var totalCandidates  = await _context.Users.CountAsync(u => u.Role == UserRole.Candidate);
        var totalCompanies   = await _context.Users.CountAsync(u => u.Role == UserRole.Company);
        var newUsersLast30d  = await _context.Users.CountAsync(u => u.CreatedAt >= thirtyDaysAgo);

        var totalOffers      = await _context.JobOffers.CountAsync();
        var offersOpen       = await _context.JobOffers.CountAsync(o => o.Status == OfferStatus.Open);
        var offersTestSent   = await _context.JobOffers.CountAsync(o => o.Status == OfferStatus.TestSent);
        var offersCompleted  = await _context.JobOffers.CountAsync(o => o.Status == OfferStatus.Completed);
        var offersCancelled  = await _context.JobOffers.CountAsync(o => o.Status == OfferStatus.Cancelled);
        var offersExpired    = await _context.JobOffers.CountAsync(o => o.Status == OfferStatus.Expired);

        var totalMatches     = await _context.Matches.CountAsync();
        var selected         = await _context.Matches.CountAsync(m => m.Stage == MatchStage.Selected);
        var rejected         = await _context.Matches.CountAsync(m => m.Stage == MatchStage.Rejected);

        var totalSubs        = await _context.TestSubmissions.CountAsync();
        var evaluated        = await _context.TestSubmissions.CountAsync(s => s.Status == SubmissionStatus.Evaluated);
        var expired          = await _context.TestSubmissions.CountAsync(s => s.Status == SubmissionStatus.Expired);
        var avgScore         = await _context.TestSubmissions
            .Where(s => s.Score != null)
            .AverageAsync(s => (double?)s.Score) ?? 0;

        var totalRevenue     = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Succeeded)
            .SumAsync(p => (decimal?)p.AmountCop) ?? 0m;
        var paymentsOk       = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Succeeded);
        var paymentsPending  = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Pending);

        double completionRate = totalSubs > 0 ? Math.Round((double)evaluated / totalSubs * 100, 1) : 0;
        double selectionRate  = totalMatches > 0 ? Math.Round((double)selected / totalMatches * 100, 1) : 0;

        var kpis = new (string Label, string Value)[]
        {
            ("— USUARIOS —",                    ""),
            ("Total candidatos",                totalCandidates.ToString()),
            ("Total empresas",                  totalCompanies.ToString()),
            ("Nuevos usuarios (últimos 30 días)", newUsersLast30d.ToString()),
            ("",                                ""),
            ("— OFERTAS —",                     ""),
            ("Total ofertas",                   totalOffers.ToString()),
            ("Activas (Open)",                  offersOpen.ToString()),
            ("Test enviado",                    offersTestSent.ToString()),
            ("Completadas",                     offersCompleted.ToString()),
            ("Canceladas",                      offersCancelled.ToString()),
            ("Expiradas",                       offersExpired.ToString()),
            ("",                                ""),
            ("— MATCHING —",                    ""),
            ("Total matches",                   totalMatches.ToString()),
            ("Candidatos seleccionados",        selected.ToString()),
            ("Candidatos rechazados",           rejected.ToString()),
            ("Tasa de selección",               $"{selectionRate}%"),
            ("",                                ""),
            ("— TESTS —",                       ""),
            ("Total submissions",               totalSubs.ToString()),
            ("Evaluadas por IA",                evaluated.ToString()),
            ("Expiradas sin respuesta",         expired.ToString()),
            ("Tasa de completación",            $"{completionRate}%"),
            ("Puntaje promedio plataforma",     $"{Math.Round(avgScore, 1)}"),
            ("",                                ""),
            ("— INGRESOS —",                    ""),
            ("Revenue total (COP)",             totalRevenue.ToString("N0")),
            ("Pagos completados",               paymentsOk.ToString()),
            ("Pagos pendientes",                paymentsPending.ToString()),
        };

        int r = 2;
        foreach (var (label, value) in kpis)
        {
            ws.Cell(r, 1).Value = label;
            ws.Cell(r, 2).Value = value;
            if (label.StartsWith("—"))
            {
                ws.Cell(r, 1).Style.Font.Bold = true;
                ws.Cell(r, 1).Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                ws.Cell(r, 2).Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
            }
            r++;
        }

        ws.Column(1).Width = 40;
        ws.Column(2).Width = 25;
    }

    private async Task BuildCompaniesSheet(XLWorkbook workbook)
    {
        var ws = workbook.Worksheets.Add("Empresas");

        var headers = new[]
        {
            "Empresa", "Email", "Miembro desde",
            "Total ofertas", "Activas", "Completadas", "Canceladas",
            "Total matches", "Seleccionados", "Total pagado (COP)"
        };
        WriteHeaders(ws, headers);

        var companies = await _context.CompanyProfiles
            .Include(c => c.User)
            .Include(c => c.JobOffers)
                .ThenInclude(o => o.Matches)
            .Include(c => c.JobOffers)
                .ThenInclude(o => o.Payment)
            .OrderBy(c => c.CompanyName)
            .ToListAsync();

        int row = 2;
        foreach (var c in companies)
        {
            var offerList  = c.JobOffers.ToList();
            var active     = offerList.Count(o => o.Status == OfferStatus.Open || o.Status == OfferStatus.TestSent);
            var completed  = offerList.Count(o => o.Status == OfferStatus.Completed);
            var cancelled  = offerList.Count(o => o.Status == OfferStatus.Cancelled);
            var allMatches = offerList.SelectMany(o => o.Matches).ToList();
            var allSelected = allMatches.Count(m => m.Stage == MatchStage.Selected);
            var totalPaid  = offerList
                .Where(o => o.Payment?.Status == PaymentStatus.Succeeded)
                .Sum(o => o.Payment!.AmountCop);

            ws.Cell(row, 1).Value  = c.CompanyName ?? "—";
            ws.Cell(row, 2).Value  = c.User.Email;
            ws.Cell(row, 3).Value  = c.CreatedAt.ToString("dd/MM/yyyy");
            ws.Cell(row, 4).Value  = offerList.Count;
            ws.Cell(row, 5).Value  = active;
            ws.Cell(row, 6).Value  = completed;
            ws.Cell(row, 7).Value  = cancelled;
            ws.Cell(row, 8).Value  = allMatches.Count;
            ws.Cell(row, 9).Value  = allSelected;
            ws.Cell(row, 10).Value = (double)totalPaid;
            ws.Cell(row, 10).Style.NumberFormat.Format = "#,##0";
            row++;
        }

        FormatSheet(ws, headers.Length, row - 1);
    }

    private async Task BuildPaymentsSheet(XLWorkbook workbook)
    {
        var ws = workbook.Worksheets.Add("Pagos");

        var headers = new[]
        {
            "Empresa", "Email empresa", "Oferta", "Tier",
            "Monto (COP)", "Estado", "Pagado el", "Transaction ID"
        };
        WriteHeaders(ws, headers);

        var payments = await _context.Payments
            .Include(p => p.JobOffer)
                .ThenInclude(o => o.CompanyProfile)
                    .ThenInclude(cp => cp.User)
            .Include(p => p.PricingTier)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        int row = 2;
        foreach (var p in payments)
        {
            var company = p.JobOffer.CompanyProfile;

            ws.Cell(row, 1).Value  = company.CompanyName ?? "—";
            ws.Cell(row, 2).Value  = company.User.Email;
            ws.Cell(row, 3).Value  = p.JobOffer.Title;
            ws.Cell(row, 4).Value  = p.PricingTier.Name;
            ws.Cell(row, 5).Value  = (double)p.AmountCop;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
            ws.Cell(row, 6).Value  = p.Status.ToString();
            ws.Cell(row, 7).Value  = p.PaidAt.HasValue ? p.PaidAt.Value.ToString("dd/MM/yyyy HH:mm") : "Pendiente";
            ws.Cell(row, 8).Value  = p.PaymentTransactionId ?? "—";
            row++;
        }

        FormatSheet(ws, headers.Length, row - 1);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void WriteHeaders(IXLWorksheet ws, IReadOnlyList<string> headers)
    {
        for (int i = 0; i < headers.Count; i++)
            ws.Cell(1, i + 1).Value = headers[i];
        StyleHeaderRow(ws, 1, headers.Count);
    }

    private static void StyleHeaderRow(IXLWorksheet ws, int row, int colCount)
    {
        var range = ws.Range(row, 1, row, colCount);
        range.Style.Font.Bold            = true;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E3A5F");
        range.Style.Font.FontColor       = XLColor.White;
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    private static void FormatSheet(IXLWorksheet ws, int colCount, int lastDataRow)
    {
        if (lastDataRow >= 2)
        {
            var dataRange = ws.Range(2, 1, lastDataRow, colCount);
            dataRange.Style.Alignment.WrapText = false;

            for (int r = 2; r <= lastDataRow; r++)
            {
                if (r % 2 == 0)
                    ws.Range(r, 1, r, colCount).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F6FC");
            }
        }

        for (int c = 1; c <= colCount; c++)
            ws.Column(c).AdjustToContents(1, Math.Max(lastDataRow, 1));

        ws.SheetView.FreezeRows(1);
    }

    private static byte[] ToBytes(XLWorkbook workbook)
    {
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private sealed class FeedbackWrapper
    {
        public string? Feedback { get; set; }
    }
}
