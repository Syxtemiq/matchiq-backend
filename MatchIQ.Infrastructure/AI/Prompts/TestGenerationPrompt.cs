using MatchIQ.Domain.Entities;
using MatchIQ.Domain.Enums;

namespace MatchIQ.Infrastructure.AI.Prompts;

public static class TestGenerationPrompt
{
    public static string Build(JobOffer offer, TestLanguage language)
    {
        var skills = offer.OfferSkills.Select(s => s.Skill.Name).ToList();
        var categories = offer.OfferCategories.Select(c => c.Category.Name).ToList();

        return language == TestLanguage.English
            ? BuildEnglish(offer, skills, categories)
            : BuildSpanish(offer, skills, categories);
    }

    private static string BuildSpanish(JobOffer offer, List<string> skills, List<string> categories)
    {
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

    private static string BuildEnglish(JobOffer offer, List<string> skills, List<string> categories)
    {
        var skillsText = skills.Count > 0
            ? string.Join(", ", skills)
            : "general software development technologies";

        var categoriesText = categories.Count > 0
            ? string.Join(", ", categories)
            : "software development";

        var englishText = offer.RequiredEnglishLevel.HasValue
            ? $"Required English level: {offer.RequiredEnglishLevel}"
            : "No specific English level required";

        var expText = offer.MinExperienceYears.HasValue
            ? $"{offer.MinExperienceYears} years of minimum experience"
            : "No minimum experience requirement";

        return $$"""
            You are a senior technical assessor. Generate a technical test in English for the following job opening.

            === JOB OPENING ===
            Title: {{offer.Title}}
            Description: {{offer.Description ?? "(no additional description)"}}
            Categories: {{categoriesText}}
            Required skills: {{skillsText}}
            Experience: {{expText}}
            {{englishText}}

            === INSTRUCTIONS ===
            Generate exactly 11 questions with this distribution:
            1. Question 1: MUST be of type "code_challenge" — a practical programming exercise contextual to the role. Use the programming language most relevant to the profile (detected from the skills). The function must be useful and non-trivial (example: calculate parking fees, process orders, validate business rules).
            2. Questions 2–8 (7 total): standard technical multiple choice about the job's skills and categories.
            3. Questions 9–10 (2 total): "scenario" multiple choice with subtle distractors to measure real judgment.
            4. Question 11 (1 total): a "gorilla" question — a question where the correct answer seems obvious but there's a subtle error or unusual behavior detail that requires attention to detail. Mark is_gorilla: true.

            === RULES ===
            - All questions in English, except code or technical terms.
            - Multiple choice questions have exactly 4 options: A, B, C, D.
            - correct_answer is the letter of the correct option (A, B, C or D).
            - explanation briefly explains WHY that is the correct answer.
            - For gorilla questions, gorilla_hint describes the detail that makes it a gorilla question.
            - The test title must reflect the role and technologies.
            - time_limit_minutes: between 30 and 60 depending on complexity.

            === RESPONSE FORMAT ===
            Respond ONLY with valid JSON, no additional text or markdown blocks.

            {
              "title": "string",
              "time_limit_minutes": number,
              "questions": [
                {
                  "order_index": 1,
                  "question_type": "code_challenge",
                  "question_text": "string — problem statement",
                  "language": "string — python | javascript | csharp | java | etc.",
                  "function_signature": "string — signature of the function to implement",
                  "example_input": "string — input examples",
                  "expected_behavior": "string — description of the expected behavior",
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
