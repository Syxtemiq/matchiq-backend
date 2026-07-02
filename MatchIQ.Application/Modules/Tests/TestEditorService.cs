using System.Text.Json;
using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Common.Interfaces.Repositories;
using MatchIQ.Application.Modules.Tests.Dtos;
using MatchIQ.Domain.Entities;
using MatchIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MatchIQ.Application.Modules.Tests;

public class TestEditorService
{
    private readonly IAppDbContext _context;
    private readonly ITestRepository _testRepository;
    private readonly IAIService _aiService;

    public TestEditorService(IAppDbContext context, ITestRepository testRepository, IAIService aiService)
    {
        _context = context;
        _testRepository = testRepository;
        _aiService = aiService;
    }

    public async Task<EditQuestionResponseDto> SendMessageAsync(int questionId, int userId, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new InvalidOperationException("El mensaje no puede estar vacío.");

        var question = await _testRepository.GetQuestionByIdAsync(questionId)
            ?? throw new KeyNotFoundException("Pregunta no encontrada.");

        // Verificar que la empresa tenga acceso a esta pregunta
        await VerifyCompanyAccessAsync(userId, question);

        // Bloquear edición si el test ya fue enviado a candidatos
        var offerStatus = await _context.JobOffers
            .Where(o => o.Test != null && o.Test.Id == question.TestId)
            .Select(o => o.Status)
            .FirstOrDefaultAsync();

        if (offerStatus != OfferStatus.PendingPayment)
            throw new InvalidOperationException("No se pueden modificar preguntas una vez que la oferta ha sido activada.");

        var testLanguage = await _context.Tests
            .Where(t => t.Id == question.TestId)
            .Select(t => t.TestLanguage)
            .FirstOrDefaultAsync();

        var history = question.QuestionChatMessages
            .OrderBy(m => m.CreatedAt)
            .ToList();

        // Guardar el mensaje del admin antes de llamar a la IA
        await _testRepository.AddChatMessageAsync(new QuestionChatMessage
        {
            QuestionId = questionId,
            Role = ChatRole.Admin,
            Content = message
        });

        // La IA recibe la pregunta actual, el historial completo y el nuevo mensaje
        var regenerated = await _aiService.RegenerateQuestionAsync(question, history, message, testLanguage);

        // Actualizar la pregunta con la versión regenerada por la IA
        question.QuestionText = regenerated.QuestionText;
        question.Explanation = regenerated.Explanation;
        question.IsGorilla = regenerated.IsGorilla;
        question.GorillaHint = regenerated.GorillaHint;
        question.OptionsJson = regenerated.Options is not null
            ? JsonSerializer.Serialize(regenerated.Options)
            : question.OptionsJson;
        question.CorrectAnswer = regenerated.CorrectAnswer ?? question.CorrectAnswer;

        if (question.QuestionType == QuestionType.CodeChallenge)
        {
            question.Language = regenerated.Language ?? question.Language;
            question.FunctionSignature = regenerated.FunctionSignature ?? question.FunctionSignature;
            question.ExampleInput = regenerated.ExampleInput ?? question.ExampleInput;
            question.ExpectedBehavior = regenerated.ExpectedBehavior ?? question.ExpectedBehavior;
        }

        await _testRepository.UpdateQuestionAsync(question);

        // Guardar la respuesta de la IA en el historial
        var assistantMessage = $"He actualizado la pregunta según tu solicitud.";
        await _testRepository.AddChatMessageAsync(new QuestionChatMessage
        {
            QuestionId = questionId,
            Role = ChatRole.Assistant,
            Content = assistantMessage
        });

        return new EditQuestionResponseDto
        {
            UpdatedQuestion = MapToDto(question),
            AssistantMessage = assistantMessage
        };
    }

    public async Task<List<ChatMessageDto>> GetChatHistoryAsync(int questionId, int userId)
    {
        var question = await _testRepository.GetQuestionByIdAsync(questionId)
            ?? throw new KeyNotFoundException("Pregunta no encontrada.");

        await VerifyCompanyAccessAsync(userId, question);

        var history = await _testRepository.GetChatHistoryAsync(questionId);

        return history
            .Select(m => new ChatMessageDto
            {
                Role = m.Role.ToString(),
                Content = m.Content,
                CreatedAt = m.CreatedAt
            })
            .ToList();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private async Task VerifyCompanyAccessAsync(int userId, TestQuestion question)
    {
        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Perfil de empresa no encontrado.");

        // La ruta: TestQuestion → Test → JobOffer → CompanyProfile
        var offer = await _context.JobOffers
            .FirstOrDefaultAsync(o => o.Test != null && o.Test.Id == question.TestId);

        if (offer is null || offer.CompanyId != company.Id)
            throw new UnauthorizedAccessException("No tienes acceso a esta pregunta.");
    }

    private static QuestionDto MapToDto(TestQuestion q)
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
            CorrectAnswer = q.CorrectAnswer,
            Explanation = q.Explanation,
            IsGorilla = q.IsGorilla,
            GorillaHint = q.GorillaHint,
            Language = q.Language,
            FunctionSignature = q.FunctionSignature,
            ExampleInput = q.ExampleInput,
            ExpectedBehavior = q.ExpectedBehavior
        };
    }
}
