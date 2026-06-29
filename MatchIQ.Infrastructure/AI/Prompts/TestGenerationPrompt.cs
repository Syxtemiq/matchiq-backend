using MatchIQ.Domain.Entities;

namespace MatchIQ.Infrastructure.AI.Prompts;

public static class TestGenerationPrompt
{
    public static string Build(JobOffer offer)
    {
        var skills = offer.OfferSkills.Select(s => s.Skill.Name).ToList();
        var categories = offer.OfferCategories.Select(c => c.Category.Name).ToList();

        var skillsText = skills.Count > 0
            ? string.Join(", ", skills)
            : "tecnologías generales de desarrollo de software";

        var categoriesText = categories.Count > 0
            ? string.Join(", ", categories)
            : "desarrollo de software";

        var englishText = offer.RequiredEnglishLevel.HasValue
            ? $"Nivel de inglés requerido: {offer.RequiredEnglishLevel}"
            : "No se requiere nivel específico de inglés";

        var expText = offer.MinExperienceYears.HasValue
            ? $"{offer.MinExperienceYears} años de experiencia mínima"
            : "Sin requisito mínimo de experiencia";

        return $$"""
            Eres un evaluador técnico senior. Genera un test técnico en español para la siguiente vacante de trabajo.

            === VACANTE ===
            Título: {{offer.Title}}
            Descripción: {{offer.Description ?? "(sin descripción adicional)"}}
            Categorías: {{categoriesText}}
            Skills requeridos: {{skillsText}}
            Experiencia: {{expText}}
            {{englishText}}

            === INSTRUCCIONES ===
            Genera exactamente 11 preguntas con esta distribución:
            1. Pregunta 1: OBLIGATORIAMENTE de tipo "code_challenge" — un ejercicio de programación práctico y contextual al rol. Usa el lenguaje de programación más relevante para el perfil (detectado de los skills). La función debe ser útil y no trivial (ejemplo: calcular cobro de parqueadero, procesar pedidos, validar reglas de negocio).
            2. Preguntas 2–8 (7 en total): múltiple opción técnicas estándar sobre los skills y categorías de la vacante.
            3. Preguntas 9–10 (2 en total): múltiple opción tipo "escenario" con distractores sutiles para medir criterio real.
            4. Preguntas 11 (1 en total): pregunta "gorilla" — una pregunta donde la respuesta correcta parece obvia pero hay un error sutil o un detalle de comportamiento inusual que requiere atención al detalle. Marca is_gorilla: true.

            === REGLAS ===
            - Todas las preguntas en español, excepto código o términos técnicos.
            - Las preguntas de múltiple opción tienen exactamente 4 opciones: A, B, C, D.
            - correct_answer es la letra de la opción correcta (A, B, C o D).
            - explanation explica brevemente POR QUÉ esa es la respuesta correcta.
            - Para preguntas gorilla, gorilla_hint describe el detalle que hace que sea una gorilla question.
            - El título del test debe reflejar el rol y las tecnologías.
            - time_limit_minutes: entre 30 y 60 según complejidad.

            === FORMATO DE RESPUESTA ===
            Responde ÚNICAMENTE con JSON válido, sin texto adicional ni bloques markdown.

            {
              "title": "string",
              "time_limit_minutes": número,
              "questions": [
                {
                  "order_index": 1,
                  "question_type": "code_challenge",
                  "question_text": "string — enunciado del problema",
                  "language": "string — python | javascript | csharp | java | etc.",
                  "function_signature": "string — firma de la función a implementar",
                  "example_input": "string — ejemplos de entrada",
                  "expected_behavior": "string — descripción del comportamiento esperado",
                  "explanation": "string",
                  "is_gorilla": false,
                  "gorilla_hint": null,
                  "options": null,
                  "correct_answer": null
                },
                {
                  "order_index": 2,
                  "question_type": "multiple_choice",
                  "question_text": "string",
                  "language": null,
                  "function_signature": null,
                  "example_input": null,
                  "expected_behavior": null,
                  "explanation": "string",
                  "is_gorilla": false,
                  "gorilla_hint": null,
                  "options": { "A": "string", "B": "string", "C": "string", "D": "string" },
                  "correct_answer": "A"
                }
              ]
            }
            """;
    }
}
