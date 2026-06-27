# MatchIQ — Pendientes Backend

> Última actualización: 2026-06-26

---

## 🔴 CRÍTICOS (rompen funcionalidad)

### ✅ Incompatibilidad de strings SQL vs EF Core
Script `fix_sql_case.sql` ejecutado: datos normalizados a PascalCase, funciones PL/pgSQL recreadas, trigger rehecho, índices parciales actualizados.

### ✅ `Enum.Parse` sin guard lanza 500 en `CandidateService`
Reemplazado con `Enum.TryParse` en `CandidateService.cs`. Ahora retorna 400 con mensaje claro si el valor es inválido.

### ✅ Regeneración de test sin verificar estado de la oferta
`TestService.GenerateTestAsync` ahora bloquea `forceRegenerate` si `offer.Status != Open` con error 400.

---

## 🟠 SEGURIDAD

### ✅ Email del candidato expuesto sin importar el stage en `MatchResultDto`
`MatchingService.MapToDto` ahora devuelve `Email = null` hasta que `stage = Selected`. `MatchResultDto.Email` es `string?`.

### ⚪ Race condition en `SelectCandidateAsync`
Cuenta candidatos seleccionados antes de guardar → dos requests simultáneos pueden ambos pasar el check y exceder `PositionsAvailable`.

**Solución:** Usar una query atómica con `ExecuteSqlRaw` o mover el cierre de oferta a un UPDATE condicional en SQL.

### ✅ Código de verificación de email no es criptográficamente seguro
`AuthService.GenerateSixDigitCode` ahora usa `RandomNumberGenerator.GetInt32()`.

---

## 🟡 LÓGICA DE NEGOCIO

### ⚪ Dos sistemas de timeout en paralelo e inconsistentes
- `SendTestsAsync` pone `Deadline = +72 horas`
- `SubmitAnswersAsync` valida `TimeLimitMinutes` desde `StartedAt`
- `DailyJobsService` expira por el `Deadline` (72h)

Un candidato puede ser expirado por el job antes de que venza su `TimeLimitMinutes` si empezó cerca de las 72h.

**Solución:** Definir un solo mecanismo. El recomendado: el `Deadline` se calcula como `StartedAt + TimeLimitMinutes` cuando el candidato hace `StartTest`, no al enviar. El check en `SubmitAnswers` se elimina y se confía en el `Deadline`.

### ✅ `DailyJobsService` no corre al arrancar el servidor
`ExecuteAsync` ahora llama `RunJobsAsync` inmediatamente al arrancar, antes de entrar al loop del timer.

### ✅ Google login ignora silenciosamente conflicto de rol
No aplica — el login es una sola pantalla sin campo de rol. Solo el registro es independiente por tipo de usuario.

---

## 🔵 CALIDAD / ESCALABILIDAD

### ⚪ Sin paginación en endpoints de lista
`GetMyOffersAsync`, `GetMatchesByOfferAsync`, `GetAllUsersAsync` retornan listas completas. Con volumen real es un problema de memoria y latencia.

**Solución:** Agregar parámetros `page` y `pageSize` con un máximo configurable.

### ✅ N+1 queries en `MatchRepository.RunMatchingAsync`
Ahora carga todos los matches existentes de la oferta en una sola query (`ToDictionaryAsync`) antes del loop. De N+1 queries a 2 queries fijas.

### ⚪ Chat del editor de preguntas guarda respuesta genérica
`TestEditorService.cs:71` siempre guarda `"He actualizado la pregunta según tu solicitud."` en el historial. El admin no puede ver qué hizo la IA.

**Solución:** Incluir en el mensaje del asistente un resumen de qué cambió (o retornar el diff de la pregunta), o al menos exponer el `QuestionText` actualizado en la respuesta del chat.

---

## ✅ RESUELTOS

### ✅ `SubmissionEvaluationPrompt` deserializaba `AnswersJson` como `Dictionary<string, string>` (crash 500)
`AnswersJson` se guarda como `List<AnswerItemDto>` (array JSON), pero el prompt lo intentaba leer como diccionario → `JsonException` en cada submit → ningún candidato podía enviar sus respuestas. Corregido: ahora se deserializa como `List<AnswerRaw>` y se convierte a diccionario `questionId → respuesta`.

### ✅ Empresa no podía ver el score del test de los candidatos
`LoadMatchDtosAsync` ahora carga las submissions evaluadas en batch (1 query) y las pasa a `MapToDto`. `MatchResultDto` expone `TestScore` (decimal?) y `TestFeedback` (string?) para todos los matches con `stage = TestCompleted | Selected`. `SelectCandidateAsync` también pasa la submission al retornar el match actualizado.

### ✅ Google OAuth no valida audience
`GoogleTokenValidator` ya pasa `Audience = [_clientId]`.

### ✅ `RunMatchingAsync` no verifica ownership
`MatchingService.RunMatchingAsync` ya verifica `offer.CompanyId != company.Id`.

### ✅ Ver resumen del test sin iniciarlo (candidato)
`GET /api/tests/{offerId}/candidate/preview` → devuelve `TestPreviewDto`.
`POST /api/tests/{offerId}/candidate/start` → registra `StartedAt`.

### ✅ Email al candidato cuando es seleccionado o rechazado
`SelectCandidateAsync` y `RejectCandidateAsync` con emails best-effort.

### ✅ Lógica de cierre de oferta unificada
La oferta cierra SOLO cuando la empresa selecciona suficientes candidatos en `SelectCandidateAsync`.

### ✅ Enums PostgreSQL eliminados
Todas las columnas enum convertidas a `VARCHAR`. `HasConversion<string>()` en `AppDbContext`. `UseSnakeCaseNamingConvention()` resuelve el mapeo de `Id` → `id`.

### ✅ Registro de admin bloqueado públicamente
`RegisterAsync` bloquea rol `Admin`. Solo `POST /api/admin/users` (requiere token Admin) puede crear admins.

### ✅ `POST /api/auth/resend-verification`
Endpoint implementado, con rate limiting `auth-strict` y anti-enumeración.

### ✅ API_REFERENCE.md actualizado
Todos los endpoints documentados.
