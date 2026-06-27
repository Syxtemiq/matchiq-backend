using System.Text.Json;
using MatchIQ.Application.Common.Dtos;
using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Common.Interfaces.Repositories;
using MatchIQ.Application.Modules.Tests.Dtos;
using MatchIQ.Domain.Entities;
using MatchIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MatchIQ.Application.Modules.Tests;

public class TestService
{
    private readonly IAppDbContext _context;
    private readonly ITestRepository _testRepository;
    private readonly IAIService _aiService;

    public TestService(IAppDbContext context, ITestRepository testRepository, IAIService aiService)
    {
        _context = context;
        _testRepository = testRepository;
        _aiService = aiService;
    }

    public async Task<TestDto> GenerateTestAsync(int offerId, int userId, bool forceRegenerate = false)
    {
        var offer = await LoadOfferForAIAsync(offerId)
            ?? throw new KeyNotFoundException("Oferta no encontrada.");

        await VerifyCompanyOwnershipAsync(userId, offer);

        var existing = await _testRepository.GetByOfferIdAsync(offerId);

        if (existing is not null && !forceRegenerate)
            return MapToDto(existing, includeAnswers: true);

        if (forceRegenerate && offer.Status != OfferStatus.Open)
            throw new InvalidOperationException("No se puede regenerar el test: la oferta ya tiene candidatos en proceso de evaluación.");

        // Si se fuerza regeneración, eliminar el test anterior (cascade borra preguntas y chat)
        if (existing is not null)
        {
            _context.Tests.Remove(existing);
            await _context.SaveChangesAsync();
        }

        var generated = await _aiService.GenerateTestAsync(offer);

        var test = new Test
        {
            OfferId = offerId,
            Title = generated.Title,
            TimeLimitMinutes = generated.TimeLimitMinutes
        };

        await _testRepository.CreateAsync(test);

        foreach (var q in generated.Questions.OrderBy(q => q.OrderIndex))
        {
            _context.TestQuestions.Add(MapToEntity(q, test.Id));
        }

        await _context.SaveChangesAsync();

        var saved = await _testRepository.GetByOfferIdAsync(offerId);
        return MapToDto(saved!, includeAnswers: true);
    }

    public async Task<TestDto> GetFullTestAsync(int offerId, int userId)
    {
        var offer = await _context.JobOffers
            .FirstOrDefaultAsync(o => o.Id == offerId)
            ?? throw new KeyNotFoundException("Oferta no encontrada.");

        await VerifyCompanyOwnershipAsync(userId, offer);

        var test = await _testRepository.GetByOfferIdAsync(offerId)
            ?? throw new KeyNotFoundException("Esta oferta aún no tiene un test generado.");

        return MapToDto(test, includeAnswers: true);
    }

    public async Task<TestPreviewDto> GetTestPreviewAsync(int offerId, int userId)
    {
        var test = await _testRepository.GetByOfferIdAsync(offerId)
            ?? throw new KeyNotFoundException("Test no encontrado.");

        var candidateProfile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de candidato no encontrado.");

        var submission = await _context.TestSubmissions
            .FirstOrDefaultAsync(s => s.TestId == test.Id && s.CandidateId == candidateProfile.Id)
            ?? throw new UnauthorizedAccessException("No estás invitado a rendir este test.");

        if (submission.Status == SubmissionStatus.Expired)
            throw new InvalidOperationException("El plazo para rendir este test ha expirado.");

        if (submission.Status == SubmissionStatus.Evaluated)
            throw new InvalidOperationException("Ya enviaste tus respuestas. Espera los resultados.");

        return new TestPreviewDto
        {
            TestId = test.Id,
            Title = test.Title,
            TimeLimitMinutes = test.TimeLimitMinutes,
            TotalQuestions = test.TestQuestions.Count,
            MultipleChoiceCount = test.TestQuestions.Count(q => q.QuestionType == QuestionType.MultipleChoice),
            CodeChallengeCount = test.TestQuestions.Count(q => q.QuestionType == QuestionType.CodeChallenge)
        };
    }

    public async Task<TestDto> StartTestAsync(int offerId, int userId)
    {
        var test = await _testRepository.GetByOfferIdAsync(offerId)
            ?? throw new KeyNotFoundException("Test no encontrado.");

        var candidateProfile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de candidato no encontrado.");

        var submission = await _context.TestSubmissions
            .FirstOrDefaultAsync(s => s.TestId == test.Id && s.CandidateId == candidateProfile.Id)
            ?? throw new UnauthorizedAccessException("No estás invitado a rendir este test.");

        if (submission.Status == SubmissionStatus.Expired)
            throw new InvalidOperationException("El plazo para rendir este test ha expirado.");

        if (submission.Status == SubmissionStatus.Evaluated)
            throw new InvalidOperationException("Ya enviaste tus respuestas. Espera los resultados.");

        if (submission.StartedAt is null)
        {
            submission.StartedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return MapToDto(test, includeAnswers: false);
    }

    public async Task<SubmissionResultDto> SubmitAnswersAsync(int testId, int userId, SubmitAnswersDto dto)
    {
        if (dto.Answers.Count == 0)
            throw new InvalidOperationException("Debes incluir al menos una respuesta.");

        var candidateProfile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de candidato no encontrado.");

        var submission = await _context.TestSubmissions
            .FirstOrDefaultAsync(s => s.TestId == testId && s.CandidateId == candidateProfile.Id)
            ?? throw new UnauthorizedAccessException("No tienes una submission activa para este test.");

        if (submission.Status != SubmissionStatus.Pending)
            throw new InvalidOperationException(submission.Status == SubmissionStatus.Expired
                ? "El plazo para rendir este test ha expirado."
                : "Ya enviaste tus respuestas.");

        var test = await _context.Tests
            .Include(t => t.TestQuestions.OrderBy(q => q.OrderIndex))
            .FirstOrDefaultAsync(t => t.Id == testId)
            ?? throw new KeyNotFoundException("Test no encontrado.");

        // Verificar límite de tiempo desde que el candidato inició el test
        if (submission.StartedAt.HasValue)
        {
            var elapsed = DateTime.UtcNow - submission.StartedAt.Value;
            if (elapsed.TotalMinutes > test.TimeLimitMinutes)
            {
                submission.Status = SubmissionStatus.Expired;
                await _context.SaveChangesAsync();
                throw new InvalidOperationException(
                    $"Tiempo agotado. El test debía completarse en {test.TimeLimitMinutes} minutos.");
            }
        }

        submission.AnswersJson = JsonSerializer.Serialize(dto.Answers);
        submission.SubmittedAt = DateTime.UtcNow;

        // Evaluación con IA
        var evaluation = await _aiService.EvaluateSubmissionAsync(test, submission);

        submission.Score = evaluation.Score;
        submission.Feedback = JsonSerializer.Serialize(evaluation);
        submission.Status = SubmissionStatus.Evaluated;
        submission.AiEvaluatedAt = DateTime.UtcNow;

        // Actualizar el match a TestCompleted
        var match = await _context.Matches
            .FirstOrDefaultAsync(m => m.OfferId == test.OfferId && m.CandidateId == candidateProfile.Id);

        if (match is not null)
        {
            match.Stage = MatchStage.TestCompleted;
            match.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return MapToSubmissionResult(submission, evaluation);
    }

    public async Task<List<CandidateTestSummaryDto>> GetMyTestsAsync(int userId)
    {
        var candidateProfile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de candidato no encontrado.");

        var submissions = await _context.TestSubmissions
            .Include(s => s.Test).ThenInclude(t => t.JobOffer)
            .Where(s => s.CandidateId == candidateProfile.Id)
            .OrderByDescending(s => s.Deadline)
            .ToListAsync();

        return submissions.Select(s => new CandidateTestSummaryDto
        {
            TestId = s.TestId,
            OfferId = s.Test.OfferId,
            OfferTitle = s.Test.JobOffer.Title,
            TestTitle = s.Test.Title,
            Status = s.Status.ToString(),
            StartedAt = s.StartedAt,
            Deadline = s.Deadline,
            TimeLimitMinutes = s.Test.TimeLimitMinutes,
            Score = s.Score
        }).ToList();
    }

    public async Task<SubmissionResultDto> GetSubmissionResultAsync(int testId, int userId)
    {
        var candidateProfile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de candidato no encontrado.");

        var submission = await _context.TestSubmissions
            .FirstOrDefaultAsync(s => s.TestId == testId && s.CandidateId == candidateProfile.Id)
            ?? throw new KeyNotFoundException("No tienes una submission para este test.");

        if (submission.Status == SubmissionStatus.Pending)
            throw new InvalidOperationException("Aún no has enviado tus respuestas.");

        SubmissionEvaluationDto? evaluation = null;
        if (submission.Feedback is not null)
        {
            try { evaluation = JsonSerializer.Deserialize<SubmissionEvaluationDto>(submission.Feedback); }
            catch { /* feedback malformado */ }
        }

        return MapToSubmissionResult(submission, evaluation);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private async Task<JobOffer?> LoadOfferForAIAsync(int offerId) =>
        await _context.JobOffers
            .Include(o => o.OfferSkills).ThenInclude(os => os.Skill)
            .Include(o => o.OfferCategories).ThenInclude(oc => oc.Category)
            .FirstOrDefaultAsync(o => o.Id == offerId);

    private async Task VerifyCompanyOwnershipAsync(int userId, JobOffer offer)
    {
        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        if (offer.CompanyId != company.Id)
            throw new UnauthorizedAccessException("No tienes acceso a esta oferta.");
    }

    private static TestQuestion MapToEntity(GeneratedQuestionDto q, int testId) => new()
    {
        TestId = testId,
        OrderIndex = q.OrderIndex,
        QuestionType = q.QuestionType,
        QuestionText = q.QuestionText,
        Explanation = q.Explanation,
        IsGorilla = q.IsGorilla,
        GorillaHint = q.GorillaHint,
        OptionsJson = q.Options is not null ? JsonSerializer.Serialize(q.Options) : null,
        CorrectAnswer = q.CorrectAnswer,
        Language = q.Language,
        FunctionSignature = q.FunctionSignature,
        ExampleInput = q.ExampleInput,
        ExpectedBehavior = q.ExpectedBehavior,
        UpdatedAt = DateTime.UtcNow
    };

    private static TestDto MapToDto(Test test, bool includeAnswers) => new()
    {
        Id = test.Id,
        OfferId = test.OfferId,
        Title = test.Title,
        TimeLimitMinutes = test.TimeLimitMinutes,
        CreatedAt = test.CreatedAt,
        Questions = test.TestQuestions
            .OrderBy(q => q.OrderIndex)
            .Select(q => MapQuestionToDto(q, includeAnswers))
            .ToList()
    };

    private static QuestionDto MapQuestionToDto(TestQuestion q, bool includeAnswers)
    {
        Dictionary<string, string>? options = null;
        if (q.OptionsJson is not null)
        {
            try { options = JsonSerializer.Deserialize<Dictionary<string, string>>(q.OptionsJson); }
            catch { /* ignorar */ }
        }

        return new QuestionDto
        {
            Id = q.Id,
            OrderIndex = q.OrderIndex,
            QuestionType = q.QuestionType.ToString(),
            QuestionText = q.QuestionText,
            Options = options,
            Language = q.Language,
            FunctionSignature = q.FunctionSignature,
            ExampleInput = q.ExampleInput,
            ExpectedBehavior = q.ExpectedBehavior,
            // Campos sensibles: solo para empresa
            CorrectAnswer = includeAnswers ? q.CorrectAnswer : null,
            Explanation = includeAnswers ? q.Explanation : null,
            IsGorilla = includeAnswers ? q.IsGorilla : null,
            GorillaHint = includeAnswers ? q.GorillaHint : null
        };
    }

    private static SubmissionResultDto MapToSubmissionResult(
        TestSubmission submission, SubmissionEvaluationDto? evaluation) => new()
    {
        Score = submission.Score,
        Feedback = evaluation?.Feedback,
        Status = submission.Status.ToString(),
        SubmittedAt = submission.SubmittedAt,
        AiEvaluatedAt = submission.AiEvaluatedAt,
        QuestionResults = evaluation?.QuestionResults
            .Select(qr => new QuestionResultItemDto
            {
                QuestionId = qr.QuestionId,
                IsCorrect = qr.IsCorrect,
                Feedback = qr.Feedback
            })
            .ToList() ?? []
    };
}
