using System.Text.Json;
using MatchIQ.Application.Common.Interfaces;
using MatchIQ.Application.Modules.Offers.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenAI.Interfaces;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;

namespace MatchIQ.Infrastructure.AI;

public class OfferParserService : IOfferParserService
{
    private readonly IAppDbContext _context;
    private readonly IOpenAIService _openAI;
    private readonly string _model;

    public OfferParserService(IAppDbContext context, IOpenAIService openAI, IConfiguration configuration)
    {
        _context = context;
        _openAI = openAI;
        _model = configuration["OpenAI:Model"] ?? Models.Gpt_4o_mini;
    }

    public async Task<ParsedOfferResponseDto> ParseFromDescriptionAsync(string rawDescription)
    {
        var categories = await _context.Categories
            .Include(c => c.Skills)
            .ToListAsync();

        var catalogLines = categories.SelectMany(c =>
            c.Skills.Select(s => $"  Skill ID {s.Id}: {s.Name} (Categoría ID {c.Id}: {c.Name})"));
        var catalogText = string.Join("\n", catalogLines);

        var systemPrompt = """
            Eres un asistente que extrae información estructurada de descripciones de vacantes de trabajo.
            Responde ÚNICAMENTE con JSON válido, sin texto adicional, sin bloques de código markdown.
            """;

        var userPrompt = $$"""
            Analiza la siguiente descripción de vacante y extrae los campos indicados.

            Descripción:
            {{rawDescription}}

            Catálogo disponible (usa SOLO estos IDs exactos):
            {{catalogText}}

            Devuelve exactamente este JSON (usa null si no puedes inferir el campo):
            {
              "title": "string o null",
              "modality": "Remote" | "Onsite" | "Hybrid" | null,
              "salary": número o null,
              "minExperienceYears": número entero o null,
              "requiredEnglishLevel": "A1" | "A2" | "B1" | "B2" | "C1" | "C2" | null,
              "suggestedCategoryIds": [IDs enteros de categorías relevantes del catálogo],
              "suggestedSkillIds": [IDs enteros de skills relevantes del catálogo],
              "confidenceNote": "breve explicación de qué detectaste y qué fue inferido"
            }
            """;

        var completion = await _openAI.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages =
            [
                ChatMessage.FromSystem(systemPrompt),
                ChatMessage.FromUser(userPrompt)
            ],
            Model = _model,
            MaxTokens = 800,
            Temperature = 0.2f
        });

        if (!completion.Successful || completion.Choices.Count == 0)
            throw new InvalidOperationException("El servicio de IA no pudo procesar la descripción.");

        var json = completion.Choices[0].Message.Content?.Trim() ?? "{}";

        try
        {
            var parsed = JsonSerializer.Deserialize<ParsedOfferJson>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException("La IA devolvió un JSON vacío.");

            return new ParsedOfferResponseDto
            {
                Title = parsed.Title,
                Modality = parsed.Modality,
                Salary = parsed.Salary,
                MinExperienceYears = parsed.MinExperienceYears,
                RequiredEnglishLevel = parsed.RequiredEnglishLevel,
                SuggestedCategoryIds = parsed.SuggestedCategoryIds,
                SuggestedSkillIds = parsed.SuggestedSkillIds,
                ConfidenceNote = parsed.ConfidenceNote
            };
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("La IA devolvió un formato inesperado. Intenta reformular la descripción.");
        }
    }

    private sealed class ParsedOfferJson
    {
        public string? Title { get; set; }
        public string? Modality { get; set; }
        public decimal? Salary { get; set; }
        public int? MinExperienceYears { get; set; }
        public string? RequiredEnglishLevel { get; set; }
        public List<int> SuggestedCategoryIds { get; set; } = [];
        public List<int> SuggestedSkillIds { get; set; } = [];
        public string? ConfidenceNote { get; set; }
    }
}
