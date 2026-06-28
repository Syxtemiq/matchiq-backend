using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Tests.Dtos;
using MatchIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MatchIQ.Application.Modules.Tests;

public class ProctoringService
{
    private readonly IAppDbContext _context;

    public ProctoringService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ProctoringReportDto> GetReportByMatchAsync(int matchId, int companyUserId)
    {
        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(c => c.UserId == companyUserId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        var match = await _context.Matches
            .Include(m => m.JobOffer)
            .FirstOrDefaultAsync(m => m.Id == matchId)
            ?? throw new KeyNotFoundException("Match no encontrado.");

        if (match.JobOffer.CompanyId != company.Id)
            throw new UnauthorizedAccessException("No tienes acceso a este match.");

        if (match.Stage < MatchStage.TestCompleted)
            throw new InvalidOperationException("El candidato aún no ha completado el test.");

        var test = await _context.Tests
            .FirstOrDefaultAsync(t => t.OfferId == match.OfferId)
            ?? throw new KeyNotFoundException("Test no encontrado.");

        var submission = await _context.TestSubmissions
            .FirstOrDefaultAsync(s => s.TestId == test.Id && s.CandidateId == match.CandidateId)
            ?? throw new KeyNotFoundException("Submission no encontrada.");

        var session = await _context.ProctoringSessions
            .Include(s => s.Events)
            .Where(s => s.SubmissionId == submission.Id)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("No hay reporte de proctoring para este candidato.");

        return new ProctoringReportDto
        {
            SessionId            = session.SessionId,
            Inicio               = session.Inicio,
            Fin                  = session.Fin,
            TotalFramesProcesados = session.TotalFramesProcesados,
            TotalEventos         = session.Events.Count,
            Eventos              = session.Events
                .OrderBy(e => e.Timestamp)
                .Select(e => new ProctoringEventDto
                {
                    Tipo      = e.Tipo,
                    Detalle   = e.Detalle,
                    Evidencia = e.Evidencia,
                    Timestamp = e.Timestamp
                })
                .ToList()
        };
    }
}
