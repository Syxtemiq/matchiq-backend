using System.Text.Json;
using MatchIQ.Application.Common.Dtos;
using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Domain.Entities;
using MatchIQ.Domain.Enums;
using MatchIQ.Infrastructure.AI.Prompts;
using Microsoft.Extensions.Configuration;
using OpenAI.Interfaces;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;

namespace MatchIQ.Infrastructure.AI;

public class OpenAIService : IAIService
{
    private readonly IOpenAIService _openAI;
    private readonly string _model;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OpenAIService(IOpenAIService openAI, IConfiguration configuration)
    {
        _openAI = openAI;
        _model = configuration["OpenAI:Model"] ?? Models.Gpt_4o_mini;
    }

    public async Task<GeneratedTestDto> GenerateTestAsync(JobOffer offer)
    {
        var prompt = TestGenerationPrompt.Build(offer);

        var completion = await _openAI.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages =
            [
                ChatMessage.FromSystem("Eres un evaluador técnico senior. Responde ÚNICAMENTE con JSON válido, sin markdown ni texto adicional."),
                ChatMessage.FromUser(prompt)
            ],
            Model = _model,
            MaxTokens = 4000,
            Temperature = 0.7f
        });

        var json = ExtractJson(completion, "GenerateTestAsync");

        var raw = JsonSerializer.Deserialize<GeneratedTestRaw>(json, _jsonOptions)
            ?? throw new InvalidOperationException("La IA devolvió un JSON vacío al generar el test.");

        return new GeneratedTestDto
        {
            Title = raw.Title ?? offer.Title,
            TimeLimitMinutes = raw.TimeLimitMinutes > 0 ? raw.TimeLimitMinutes : 30,
            Questions = raw.Questions.Select(MapQuestion).ToList()
        };
    }

    public async Task<GeneratedQuestionDto> RegenerateQuestionAsync(
        TestQuestion question,
        IEnumerable<QuestionChatMessage> history,
        string adminMessage)
    {
        var messages = QuestionEditPrompt.BuildMessages(question, history, adminMessage);

        var completion = await _openAI.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages = messages,
            Model = _model,
            MaxTokens = 1200,
            Temperature = 0.5f
        });

        var json = ExtractJson(completion, "RegenerateQuestionAsync");

        var raw = JsonSerializer.Deserialize<GeneratedQuestionRaw>(json, _jsonOptions)
            ?? throw new InvalidOperationException("La IA devolvió un JSON vacío al regenerar la pregunta.");

        return MapQuestion(raw);
    }

    public async Task<CandidateInsightDto> EvaluateCandidateAsync(JobOffer offer, Match match)
    {
        var prompt = EvaluationPrompt.Build(offer, match);

        var completion = await _openAI.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages =
            [
                ChatMessage.FromSystem("Eres un evaluador técnico. Responde ÚNICAMENTE con JSON válido, sin markdown ni texto adicional."),
                ChatMessage.FromUser(prompt)
            ],
            Model = _model,
            MaxTokens = 600,
            Temperature = 0.4f
        });

        var json = ExtractJson(completion, "EvaluateCandidateAsync");

        var raw = JsonSerializer.Deserialize<CandidateInsightRaw>(json, _jsonOptions)
            ?? throw new InvalidOperationException("La IA devolvió un JSON vacío al evaluar el candidato.");

        return new CandidateInsightDto
        {
            FitScore = raw.FitScore,
            Insight = raw.Insight ?? string.Empty,
            Strengths = raw.Strengths ?? [],
            Opportunities = raw.Opportunities ?? [],
            Recommendation = raw.Recommendation ?? string.Empty
        };
    }

    public async Task<SubmissionEvaluationDto> EvaluateSubmissionAsync(Test test, TestSubmission submission)
    {
        var prompt = SubmissionEvaluationPrompt.Build(test, submission);

        var completion = await _openAI.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages =
            [
                ChatMessage.FromSystem("Eres un evaluador técnico. Responde ÚNICAMENTE con JSON válido, sin markdown ni texto adicional."),
                ChatMessage.FromUser(prompt)
            ],
            Model = _model,
            MaxTokens = 2000,
            Temperature = 0.3f
        });

        var json = ExtractJson(completion, "EvaluateSubmissionAsync");

        var raw = JsonSerializer.Deserialize<SubmissionEvaluationRaw>(json, _jsonOptions)
            ?? throw new InvalidOperationException("La IA devolvió un JSON vacío al evaluar la submission.");

        return new SubmissionEvaluationDto
        {
            Score = raw.Score,
            Feedback = raw.Feedback ?? string.Empty,
            QuestionResults = (raw.QuestionResults ?? []).Select(qr => new QuestionEvaluationDto
            {
                QuestionId = qr.QuestionId,
                IsCorrect = qr.IsCorrect,
                Feedback = qr.Feedback
            }).ToList()
        };
    }

    // ── helpers ────────────────────────────────────────────────────────────────

    private static string ExtractJson(
        OpenAI.ObjectModels.ResponseModels.ChatCompletionCreateResponse completion,
        string caller)
    {
        if (!completion.Successful || completion.Choices.Count == 0)
            throw new InvalidOperationException(
                $"OpenAI no devolvió respuesta en {caller}. Error: {completion.Error?.Message}");

        var content = completion.Choices[0].Message.Content?.Trim() ?? "{}";

        // Quita bloques markdown si la IA los incluyó de todos modos
        if (content.StartsWith("```"))
        {
            var start = content.IndexOf('\n') + 1;
            var end = content.LastIndexOf("```");
            if (end > start)
                content = content[start..end].Trim();
        }

        return content;
    }

    private static GeneratedQuestionDto MapQuestion(GeneratedQuestionRaw raw)
    {
        var type = string.Equals(raw.QuestionType, "code_challenge", StringComparison.OrdinalIgnoreCase)
            ? QuestionType.CodeChallenge
            : QuestionType.MultipleChoice;

        return new GeneratedQuestionDto
        {
            QuestionType = type,
            OrderIndex = raw.OrderIndex,
            QuestionText = raw.QuestionText ?? string.Empty,
            Explanation = raw.Explanation,
            IsGorilla = raw.IsGorilla,
            GorillaHint = raw.GorillaHint,
            Options = raw.Options,
            CorrectAnswer = raw.CorrectAnswer,
            Language = raw.Language,
            FunctionSignature = raw.FunctionSignature,
            ExampleInput = raw.ExampleInput,
            ExpectedBehavior = raw.ExpectedBehavior
        };
    }

    // ── modelos de deserialización internos ───────────────────────────────────

    private sealed class GeneratedTestRaw
    {
        public string? Title { get; set; }
        public int TimeLimitMinutes { get; set; }
        public List<GeneratedQuestionRaw> Questions { get; set; } = [];
    }

    private sealed class GeneratedQuestionRaw
    {
        public int OrderIndex { get; set; }
        public string? QuestionType { get; set; }
        public string? QuestionText { get; set; }
        public string? Explanation { get; set; }
        public bool IsGorilla { get; set; }
        public string? GorillaHint { get; set; }
        public Dictionary<string, string>? Options { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? Language { get; set; }
        public string? FunctionSignature { get; set; }
        public string? ExampleInput { get; set; }
        public string? ExpectedBehavior { get; set; }
    }

    private sealed class CandidateInsightRaw
    {
        public decimal FitScore { get; set; }
        public string? Insight { get; set; }
        public List<string>? Strengths { get; set; }
        public List<string>? Opportunities { get; set; }
        public string? Recommendation { get; set; }
    }

    private sealed class SubmissionEvaluationRaw
    {
        public decimal Score { get; set; }
        public string? Feedback { get; set; }
        public List<QuestionResultRaw>? QuestionResults { get; set; }
    }

    private sealed class QuestionResultRaw
    {
        public int QuestionId { get; set; }
        public bool IsCorrect { get; set; }
        public string? Feedback { get; set; }
    }
}
