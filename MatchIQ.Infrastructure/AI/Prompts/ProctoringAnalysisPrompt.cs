using MatchIQ.Domain.Entities;

namespace MatchIQ.Infrastructure.AI.Prompts;

public static class ProctoringAnalysisPrompt
{
    public static string Build(IEnumerable<ProctoringEvent> events, decimal integrityScore)
    {
        var eventLines = events
            .OrderBy(e => e.Timestamp)
            .Select(e =>
            {
                var detail = string.IsNullOrWhiteSpace(e.Detalle) ? "" : $" — {e.Detalle}";
                return $"- [{e.Timestamp:HH:mm:ss}] {e.Tipo}{detail}";
            });

        var eventBlock = eventLines.Any()
            ? string.Join("\n", eventLines)
            : "(sin eventos detectados)";

        return $$"""
            Eres un auditor de integridad de pruebas técnicas. Analiza el comportamiento de un candidato durante un test y genera un resumen ejecutivo en español para la empresa contratante.

            === DATOS DE LA SESIÓN ===
            Score de integridad calculado: {{integrityScore:F1}} / 100
            Eventos detectados:
            {{eventBlock}}

            === INSTRUCCIONES ===
            Redacta un párrafo conciso (2-4 oraciones) explicando:
            - Qué incidencias ocurrieron y cuándo
            - Qué tan grave es la situación en términos de integridad del test
            - Una conclusión sobre la confiabilidad de los resultados

            Si no hubo eventos, indica que la sesión transcurrió sin incidentes detectados.
            Sé directo y objetivo. No uses markdown.

            Responde ÚNICAMENTE con JSON válido:
            {
              "summary": "texto del resumen aquí"
            }
            """;
    }
}
