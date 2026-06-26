using MatchIQ.Domain.Entities;

namespace MatchIQ.Infrastructure.AI.Prompts;

public static class EvaluationPrompt
{
    public static string Build(JobOffer offer, Match match)
    {
        var candidate = match.CandidateProfile;
        var offerSkills = offer.OfferSkills.Select(s => s.Skill.Name).ToList();
        var offerCategories = offer.OfferCategories.Select(c => c.Category.Name).ToList();
        var candidateSkills = candidate.CandidateSkills
            .Select(cs => $"{cs.Skill.Name} (nivel {cs.Level}/5)")
            .ToList();
        var candidateCategories = candidate.CandidateCategories
            .Select(cc => cc.Category.Name)
            .ToList();

        var matchPct = match.MatchPercentage.HasValue
            ? $"{match.MatchPercentage:F1}%"
            : "no calculado";

        return $$"""
            Eres un evaluador técnico. Analiza qué tan bien encaja este candidato con la vacante y genera un insight cualitativo en español.

            === VACANTE ===
            Título: {{offer.Title}}
            Descripción: {{offer.Description ?? "(sin descripción)"}}
            Categorías requeridas: {{string.Join(", ", offerCategories)}}
            Skills requeridos: {{string.Join(", ", offerSkills)}}
            Experiencia mínima: {{(offer.MinExperienceYears.HasValue ? $"{offer.MinExperienceYears} años" : "no especificada")}}
            Inglés requerido: {{(offer.RequiredEnglishLevel.HasValue ? offer.RequiredEnglishLevel.ToString() : "no especificado")}}

            === CANDIDATO ===
            Porcentaje de match (algoritmo SQL): {{matchPct}}
            Seniority: {{(candidate.Seniority.HasValue ? candidate.Seniority.ToString() : "no especificado")}}
            Años de experiencia: {{(candidate.ExperienceYears.HasValue ? $"{candidate.ExperienceYears}" : "no especificados")}}
            Nivel de inglés: {{(candidate.EnglishLevel.HasValue ? candidate.EnglishLevel.ToString() : "no especificado")}}
            Categorías del candidato: {{(candidateCategories.Count > 0 ? string.Join(", ", candidateCategories) : "ninguna")}}
            Skills del candidato: {{(candidateSkills.Count > 0 ? string.Join(", ", candidateSkills) : "ninguno")}}

            === INSTRUCCIONES ===
            Evalúa el fit cualitativo del candidato. Considera qué skills tiene y cuáles le faltan, su nivel de experiencia vs lo requerido, y su nivel de inglés.

            Responde ÚNICAMENTE con JSON válido:
            {
              "fit_score": número entre 0 y 10 (ej: 7.5),
              "insight": "párrafo conciso (2-3 oraciones) describiendo el fit general del candidato",
              "strengths": ["fortaleza 1", "fortaleza 2", "fortaleza 3"],
              "opportunities": ["área de mejora 1", "área de mejora 2"],
              "recommendation": "Altamente recomendado" | "Recomendado" | "Recomendado con reservas" | "No recomendado"
            }
            """;
    }
}
