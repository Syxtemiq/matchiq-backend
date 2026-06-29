using MatchIQ.Domain.Entities;
using MatchIQ.Domain.Enums;
using OpenAI.ObjectModels.RequestModels;

namespace MatchIQ.Infrastructure.AI.Prompts;

public static class QuestionEditPrompt
{
    public static string BuildSystemPrompt() => """
        Eres un editor experto de preguntas técnicas para evaluaciones de desarrollo de software.
        Tu tarea es modificar una pregunta existente según las instrucciones del administrador.

        Reglas:
        - Mantén el mismo question_type (no puedes cambiar code_challenge a multiple_choice ni viceversa).
        - Mantén el mismo order_index.
        - Aplica exactamente lo que pide el administrador, sin cambios adicionales no solicitados.
        - Responde ÚNICAMENTE con JSON válido de la pregunta modificada, sin texto adicional.

        Formato de respuesta para multiple_choice:
        {
          "order_index": número,
          "question_type": "multiple_choice",
          "question_text": "string",
          "explanation": "string",
          "is_gorilla": bool,
          "gorilla_hint": "string o null",
          "options": { "A": "string", "B": "string", "C": "string", "D": "string" },
          "correct_answer": "A" | "B" | "C" | "D",
          "language": null,
          "function_signature": null,
          "example_input": null,
          "expected_behavior": null
        }

        Formato de respuesta para code_challenge:
        {
          "order_index": número,
          "question_type": "code_challenge",
          "question_text": "string",
          "explanation": "string",
          "is_gorilla": false,
          "gorilla_hint": null,
          "options": null,
          "correct_answer": null,
          "language": "string",
          "function_signature": "string",
          "example_input": "string",
          "expected_behavior": "string"
        }
        """;

    public static List<ChatMessage> BuildMessages(
        TestQuestion currentQuestion,
        IEnumerable<QuestionChatMessage> history,
        string newAdminMessage)
    {
        var messages = new List<ChatMessage>
        {
            ChatMessage.FromSystem(BuildSystemPrompt()),
            ChatMessage.FromUser(BuildCurrentQuestionContext(currentQuestion))
        };

        foreach (var msg in history)
        {
            if (msg.Role == ChatRole.Admin)
                messages.Add(ChatMessage.FromUser(msg.Content));
            else
                messages.Add(ChatMessage.FromAssistant(msg.Content));
        }

        messages.Add(ChatMessage.FromUser(newAdminMessage));
        return messages;
    }

    private static string BuildCurrentQuestionContext(TestQuestion q)
    {
        if (q.QuestionType == QuestionType.CodeChallenge)
        {
            return $"""
                Pregunta actual (code_challenge, orden {q.OrderIndex}):
                Enunciado: {q.QuestionText}
                Lenguaje: {q.Language}
                Firma: {q.FunctionSignature}
                Ejemplo de entrada: {q.ExampleInput}
                Comportamiento esperado: {q.ExpectedBehavior}
                Explicación: {q.Explanation}
                """;
        }

        return $"""
            Pregunta actual (multiple_choice, orden {q.OrderIndex}):
            Enunciado: {q.QuestionText}
            Opciones: {q.OptionsJson}
            Respuesta correcta: {q.CorrectAnswer}
            Explicación: {q.Explanation}
            Es gorilla: {q.IsGorilla}
            {(q.IsGorilla ? $"Gorilla hint: {q.GorillaHint}" : "")}
            """;
    }
}
