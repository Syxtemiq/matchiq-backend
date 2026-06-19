namespace MatchIQ.Domain.Entities;

// Pregunta individual dentro de un test
// Tipos: MultipleChoice (opciones A-D) o CodeChallenge (el candidato escribe código)
// El admin puede solicitar cambios a la IA via QuestionChatMessage
// La IA regenera solo esta pregunta, sin tocar las demás
// Gorilla questions: preguntas con distractor oculto para medir atención
public class TestQuestion
{
    // TODO: Id, TestId, OrderIndex, Type (QuestionType enum)
    // TODO: QuestionText, OptionsJson (para MultipleChoice: {"A":..,"B":..,"C":..,"D":..})
    // TODO: CorrectAnswer, Explanation, GorillaHint (solo si es gorilla type)
    // TODO: IsGorilla (bool), Language (para CodeChallenge: "python", "javascript", etc.)
    // TODO: FunctionSignature (para CodeChallenge: ej "def calcular_cobro(horas, minutos):")
    // TODO: navegación a Test
    // TODO: colección de QuestionChatMessage
}
