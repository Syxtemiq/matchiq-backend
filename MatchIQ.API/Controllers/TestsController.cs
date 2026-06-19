namespace MatchIQ.API.Controllers;

// [ApiController]
// [Route("api/tests")]
public class TestsController // : ControllerBase
{
    // TODO: inyectar TestService, TestEditorService

    // GET api/tests/{offerId}
    // [Authorize(Roles = "Company")]
    // TODO: GetFullTestAsync(int offerId)
    //       test completo CON respuestas correctas (solo para empresa)

    // POST api/tests/{offerId}/regenerate
    // [Authorize(Roles = "Company")]
    // TODO: RegenerateTestAsync(int offerId)
    //       regenera el test completo si la empresa lo solicita

    // ── Chat de edición de preguntas ──────────────────────────────────────────

    // GET api/tests/questions/{questionId}/chat
    // [Authorize(Roles = "Company")]
    // TODO: GetChatHistoryAsync(int questionId)
    //       historial de mensajes del chat para esa pregunta

    // POST api/tests/questions/{questionId}/chat
    // [Authorize(Roles = "Company")]
    // TODO: SendChatMessageAsync(int questionId, [FromBody] ChatMessageDto dto)
    //       admin envía mensaje → IA regenera la pregunta → retorna pregunta actualizada

    // ── Endpoints para candidatos ─────────────────────────────────────────────

    // GET api/tests/{offerId}/candidate
    // [Authorize(Roles = "Candidate")]
    // TODO: GetTestForCandidateAsync(int offerId)
    //       test SIN respuestas correctas

    // POST api/tests/{testId}/submit
    // [Authorize(Roles = "Candidate")]
    // TODO: SubmitAnswersAsync(int testId, [FromBody] SubmitAnswersDto dto)

    // GET api/tests/{testId}/result
    // [Authorize(Roles = "Candidate")]
    // TODO: GetSubmissionResultAsync(int testId)
}
