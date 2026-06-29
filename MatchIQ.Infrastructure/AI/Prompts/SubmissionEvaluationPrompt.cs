using System.Text.Json;
using MatchIQ.Domain.Entities;
using MatchIQ.Domain.Enums;

namespace MatchIQ.Infrastructure.AI.Prompts;

public static class SubmissionEvaluationPrompt
{
    public static string Build(Test test, TestSubmission submission)
    {
        var questions = test.TestQuestions.OrderBy(q => q.OrderIndex).ToList();

        var answerList = submission.AnswersJson is not null
            ? JsonSerializer.Deserialize<List<AnswerRaw>>(submission.AnswersJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            : null;

        var answers = answerList?.ToDictionary(
            a => a.QuestionId.ToString(),
            a => a.SelectedOption ?? a.CodeSubmitted ?? "(sin respuesta)")
            ?? new Dictionary<string, string>();

        var questionsBlock = string.Join("\n\n", questions.Select(q =>
        {
            var candidateAnswer = answers.TryGetValue(q.Id.ToString(), out var ans) ? ans : "(sin respuesta)";

            if (q.QuestionType == QuestionType.CodeChallenge)
            {
                return $"""
                    [Pregunta {q.OrderIndex} — CodeChallenge — ID: {q.Id}]
                    Enunciado: {q.QuestionText}
                    Lenguaje: {q.Language}
                    Firma esperada: {q.FunctionSignature}
                    Comportamiento esperado: {q.ExpectedBehavior}
                    Respuesta del candidato (código escrito):
                    ---
                    {candidateAnswer}
                    ---
                    """;
            }

            var optionsText = q.OptionsJson is not null
                ? string.Join(", ", JsonSerializer
                    .Deserialize<Dictionary<string, string>>(q.OptionsJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?.Select(kv => $"{kv.Key}) {kv.Value}") ?? [])
                : "";

            return $"""
                [Pregunta {q.OrderIndex} — MultipleChoice — ID: {q.Id}]
                Enunciado: {q.QuestionText}
                Opciones: {optionsText}
                Respuesta correcta: {q.CorrectAnswer}
                Respuesta del candidato: {candidateAnswer}
                Es gorilla: {q.IsGorilla}
                """;
        }));

        return $$"""
            Eres un evaluador técnico. Evalúa la submission completa de un candidato para el siguiente test.

            === TEST: {{test.Title}} ===
            {{questionsBlock}}

            === INSTRUCCIONES ===
            1. Para cada pregunta de múltiple opción: compara la respuesta del candidato con la correcta (A/B/C/D). Es correcto si coincide exactamente.
            2. Para la pregunta de código: evalúa si el código implementado:
               - Resuelve correctamente el problema (40%)
               - Es legible y bien estructurado (30%)
               - Maneja casos borde o errores (20%)
               - Es eficiente (10%)
               Asigna is_correct: true si la implementación es sustancialmente correcta (≥ 60% de los criterios).
            3. Score global: (preguntas correctas / total preguntas) * 100. Para el código, pondera como 2 preguntas de múltiple opción.
            4. El feedback narrativo debe ser constructivo y específico, mencionando qué hizo bien y qué puede mejorar.

            Responde ÚNICAMENTE con JSON válido:
            {
              "score": número entre 0 y 100,
              "feedback": "párrafo de feedback general del candidato (3-4 oraciones)",
              "question_results": [
                {
                  "question_id": número,
                  "is_correct": bool,
                  "feedback": "observación específica sobre esta respuesta (1 oración)"
                }
              ]
            }
            """;
    }

    private sealed class AnswerRaw
    {
        public int QuestionId { get; set; }
        public string? SelectedOption { get; set; }
        public string? CodeSubmitted { get; set; }
    }
}
