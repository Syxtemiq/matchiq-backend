namespace MatchIQ.Infrastructure.AI.Prompts;

// Construye el prompt para generar el test completo de una oferta
// Distribución de preguntas:
//   - 1 CodeChallenge (siempre primera): función simple en el lenguaje de la oferta
//     ejemplo: calcular cobro de parqueadero por horas y minutos
//   - 6 preguntas técnicas estándar relacionadas al rol
//   - 2 preguntas scenario con distractor sutil
//   - 2 preguntas gorilla (anomalía obvia escondida a plena vista)
public static class TestGenerationPrompt
{
    // TODO: Build(JobOffer offer) → string
    //       incluye título, descripción, experiencia, inglés requerido
    //       especifica el lenguaje para el CodeChallenge (detectado de la oferta)
    //       describe la firma de la función del CodeChallenge
    //       especifica formato JSON de respuesta esperado
}
