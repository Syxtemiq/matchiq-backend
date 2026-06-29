# MatchIQ — Pendientes Backend

> Última actualización: 2026-06-27

---

## 🔴 CRÍTICOS (rompen datos o lógica central)

### ✅ Incompatibilidad de strings SQL vs EF Core
Script `fix_sql_case.sql` ejecutado: datos normalizados a PascalCase, funciones PL/pgSQL recreadas, trigger rehecho, índices parciales actualizados.

### ✅ `Enum.Parse` sin guard lanza 500 en `CandidateService`o 
Reemplazado con `Enum.TryParse` en `CandidateService.cs`. Ahora retorna 400 con mensaje claro si el valor es inválido.

### ✅ Regeneración de test sin verificar estado de la oferta
`TestService.GenerateTestAsync` ahora bloquea `forceRegenerate` si `offer.Status != Open` con error 400.

### ✅ `SubmissionEvaluationPrompt` deserializaba `AnswersJson` como `Dictionary<string, string>` (crash 500)
`AnswersJson` se guarda como `List<AnswerItemDto>` (array JSON), pero el prompt lo intentaba leer como diccionario → `JsonException` en cada submit → ningún candidato podía enviar sus respuestas. Corregido: ahora se deserializa como `List<AnswerRaw>` y se convierte a diccionario `questionId → respuesta`.

### ✅ BUG-01 — Fórmula `AdjustedScore` matemáticamente incorrecta (`MatchingService.cs:68, 229`)
`FitScore` está en escala 0–10. La fórmula `0.1 * fitScore * 100` le daba hasta 100 puntos al componente IA (peso del 100% en vez del 10%). Scores inflados para todos los candidatos. Corregido a `0.9 * matchPercentage + fitScore * 10`.

### ✅ BUG-02 — Respuestas del candidato se pierden si la IA falla (`TestService.cs:165–186`)
`AnswersJson` y `SubmittedAt` se seteaban antes de la llamada a la IA pero `SaveChangesAsync` solo ocurría después. Si OpenAI lanzaba excepción, las respuestas desaparecían y el test quedaba en `Pending`. Corregido: `SaveChangesAsync` inmediato tras setear respuestas, luego IA, luego segundo save.

### ✅ BUG-06 — `CandidateCategories` nunca incluida → IA evalúa con "categorías: ninguna"
`MatchRepository.GetByOfferAsync` y `MatchingService.ReevaluateAsync` incluían `CandidateSkills` pero no `CandidateCategories`. El prompt de evaluación siempre recibía lista vacía. Corregido: `.ThenInclude(cp => cp.CandidateCategories).ThenInclude(cc => cc.Category)` agregado en ambos lugares.

### ✅ BUG-03 — Cancel endpoint responde "Oferta cancelada" aunque no cancele (`OffersController.cs:85`)
Cuando hay candidatos en proceso, el servicio retorna `{ Cancelled: false, Warning: "..." }` pero el controller ignoraba eso y siempre respondía 200 "Oferta cancelada correctamente." Frontend no podía distinguir warning de éxito. Corregido: el controller revisa `result.Cancelled` y retorna 409 con el warning si no se canceló.

---

## 🟠 SEGURIDAD

### ✅ Email del candidato expuesto sin importar el stage en `MatchResultDto`
`MatchingService.MapToDto` ahora devuelve `Email = null` hasta que `stage = Selected`.

### ✅ Código de verificación de email no es criptográficamente seguro
`AuthService.GenerateSixDigitCode` ahora usa `RandomNumberGenerator.GetInt32()`.

### ✅ Google OAuth no valida audience
`GoogleTokenValidator` ya pasa `Audience = [_clientId]`.

### 🚫 SEC-01 — CORS `AllowAnyOrigin` — descartado intencionalmente
Decisión del equipo: CORS abierto por ahora, no se restringe.

### 🚫 SEC-02 — Swagger siempre activo — descartado intencionalmente
Decisión del equipo: Swagger habilitado en todos los entornos, no se restringe por ambiente.

### ⚪ SEC-03 — Refresh tokens en texto plano en BD (`JwtService.cs:51`)
Si la BD se compromete, todas las sesiones activas son inmediatamente usables. Solución: almacenar SHA-256 del token y comparar en lookup.

### ⚪ BUG-05 — Race condition en `SelectCandidateAsync` (`MatchingService.cs:322–332`)
Dos requests simultáneos pueden ambos pasar el check de `PositionsAvailable` y exceder el cupo. Solución: UPDATE condicional en SQL o `ExecuteSqlRaw` con lock.

---

## 🟡 LÓGICA DE NEGOCIO

### ✅ Dos sistemas de timeout en paralelo e inconsistentes
Unificado: `SendTest → Deadline = now + TestDeadlineDays`; `StartTest → Deadline = StartedAt + TimeLimitMinutes`. Solo el `Deadline` controla el vencimiento. El check manual de tiempo en `SubmitAnswers` fue eliminado.

### ✅ `DailyJobsService` no corre al arrancar el servidor
`ExecuteAsync` ahora llama `RunJobsAsync` inmediatamente al arrancar.

### ✅ Google login ignora silenciosamente conflicto de rol
No aplica — el login es una sola pantalla sin campo de rol. Solo el registro es independiente por tipo de usuario.

### 🚫 LOGIC-01 — Actualizar skills/categorías post-pago — descartado intencionalmente
Decisión del negocio: el pago es el punto de no retorno. En `PendingPayment` se puede editar todo libremente. Una vez pagada (`Open`), la oferta queda inmutable.

### ✅ LOGIC-03 — Punto de no retorno en el pago
`UpdateOfferAsync`, `TestEditorService.SendMessageAsync` y `forceRegenerate` solo están permitidos en `PendingPayment`. Una vez que la oferta pasa a `Open` (pagada), nada puede modificarse.

### ✅ LOGIC-04 — TestEditorService bloquea edición tras el pago
`SendMessageAsync` ahora verifica `offer.Status == PendingPayment`. Si la oferta ya fue pagada (`Open` en adelante), lanza 400.

### ✅ LOGIC-05 — GenerateTest/Regenerate bloqueado tras el pago
`forceRegenerate = true` solo está permitido en `PendingPayment`. En `Open` o posterior, la empresa no puede regenerar el test.

### ⚪ LOGIC-05 — `GenerateTest` se puede llamar con oferta en `PendingPayment`
Solo bloquea `forceRegenerate`. Consume tokens de OpenAI en ofertas sin pagar. Solución: agregar guard `offer.Status == Open || offer.Status == TestSent` al inicio de `GenerateTestAsync`.

### ⚪ LOGIC-06 — Admin puede borrar empresa con ofertas activas (`AdminService.cs:139`)
Cascade destruye ofertas, matches, submissions y pagos sin warning. Solución: verificar ofertas activas antes de borrar y retornar error o requerir confirmación explícita.

### ⚪ LOGIC-07 — `SendTestsAsync` puede ejecutarse con oferta en estado `Expired`
No verifica `offer.ExpiresAt` ni bloquea el envío si la oferta no está en `Open` o `TestSent`.

### 🚫 MISSING-02 — Candidato no ve sus matches — descartado intencionalmente
Decisión del negocio: el proceso es opaco para el candidato. Solo recibe correos (test enviado, seleccionado, rechazado). No hay vista de matches ni etapas.

---

## 🔵 CALIDAD / ESCALABILIDAD

### ⚪ Sin paginación en endpoints de lista
`GetMyOffersAsync`, `GetMatchesByOfferAsync`, `GetAllUsersAsync` retornan listas completas. Agregar `page` y `pageSize`.

### ✅ N+1 queries en `MatchRepository.RunMatchingAsync`
Ahora carga todos los matches existentes de la oferta en una sola query (`ToDictionaryAsync`) antes del loop.

### ⚪ Chat del editor guarda respuesta genérica (`TestEditorService.cs:71`)
Siempre guarda `"He actualizado la pregunta según tu solicitud."` sin incluir qué cambió la IA. Exponer el `QuestionText` actualizado en la respuesta.

### ⚪ DB-05 — `WompiService` inyecta `AppDbContext` concreto en vez de `IAppDbContext`
Rompe la abstracción de la capa de Infrastructure. Solución: cambiar a `IAppDbContext`.

### ⚪ MISSING-03/04 — Dead code: `IMatchRepository.UpdateStageAsync` y `JobOfferRepository.CreateAsync/UpdateAsync`
Nunca se llaman. Se pueden eliminar para reducir deuda técnica.

---

## ✅ RESUELTOS (histórico)

### ✅ Empresa no podía ver el score del test de los candidatos
`LoadMatchDtosAsync` ahora carga las submissions evaluadas en batch. `MatchResultDto` expone `TestScore` y `TestFeedback`.

### ✅ `RunMatchingAsync` no verifica ownership
`MatchingService.RunMatchingAsync` ya verifica `offer.CompanyId != company.Id`.

### ✅ Ver resumen del test sin iniciarlo (candidato)
`GET /api/tests/{offerId}/candidate/preview` → devuelve `TestPreviewDto`.

### ✅ Email al candidato cuando es seleccionado o rechazado
`SelectCandidateAsync` y `RejectCandidateAsync` con emails best-effort.

### ✅ Lógica de cierre de oferta unificada
La oferta cierra ONLY cuando la empresa selecciona suficientes candidatos en `SelectCandidateAsync`.

### ✅ Enums PostgreSQL eliminados
Columnas enum convertidas a `VARCHAR`. `HasConversion<string>()` en `AppDbContext`.

### ✅ Registro de admin bloqueado públicamente
`RegisterAsync` bloquea rol `Admin`. Solo `POST /api/admin/users` (requiere token Admin) puede crear admins.

### ✅ `POST /api/auth/resend-verification`
Endpoint implementado con rate limiting y anti-enumeración.

### ✅ API_REFERENCE.md actualizado
Todos los endpoints documentados incluyendo `TestScore`, `TestFeedback`, `TestDeadlineDays`, `TimeLimitMinutes`, y endpoint `GET /api/tests/candidate`.
