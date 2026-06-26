namespace MatchIQ.Infrastructure.AI.Prompts;

// Este archivo ya no se usa como prompt independiente.
// La pregunta de código se genera como parte de TestGenerationPrompt.Build(),
// dentro del bloque de instrucciones para la pregunta de order_index = 1.
// Se mantiene el archivo por si se necesita regenerar solo la CodeChallenge en el futuro.
public static class CodeQuestionPrompt
{
    public static string Build(string language, string offerTitle) => $$"""
        Genera una pregunta de programación práctica en {{language}} para una vacante de {{offerTitle}}.

        Requisitos:
        - El ejercicio debe ser contextual al rol (no un algoritmo genérico de entrevista).
        - Dificultad media: suficiente para diferenciar candidatos, no imposible.
        - Incluye una firma de función clara y ejemplos de entrada/salida.
        - El candidato escribe el cuerpo de la función; no se ejecuta en sandbox — la IA evalúa el texto.

        Responde ÚNICAMENTE con JSON válido:
        {
          "question_text": "string — enunciado del problema",
          "language": "{{language}}",
          "function_signature": "string — firma completa de la función",
          "example_input": "string — ejemplos de llamada con entrada y salida esperada",
          "expected_behavior": "string — descripción del comportamiento correcto",
          "explanation": "string — qué se evalúa y qué espera la solución ideal"
        }
        """;
}
