# MatchIQ — Pendientes Backend

> Última actualización: 2026-06-26

---

## 🔴 CRÍTICOS (rompen funcionalidad)

### ⚪ Incompatibilidad de strings SQL vs EF Core
EF Core `HasConversion<string>()` guarda enums en PascalCase (`'Open'`, `'Pending'`, `'Matched'`), pero las funciones PostgreSQL comparan contra minúsculas (`'open'`, `'pending'`, `'matched'`). Consecuencias:
- `trigger_rematch_open_offers` → nunca dispara → matching automático muerto
- `expire_stale_offers()` → nunca encuentra registros → ofertas no expiran
- `expire_stale_submissions()` → submissions no expiran
- `get_full_offer_ranking` inserta `stage = 'matched'` (minúscula) → inconsistencia

**Solución:** Ejecutar `CREATE OR REPLACE FUNCTION` con valores PascalCase en la BD (ya están en `DBContext.md`). No requiere cambios en C#.

### ⚪ `Enum.Parse` sin guard lanza 500 en `CandidateService`
`CandidateService.cs:84-87` usa `Enum.Parse` (no `TryParse`). Lanza `ArgumentException`, que el middleware no captura → 500 en vez de 400.

**Solución:** Reemplazar con `Enum.TryParse` igual que en `AuthService` y `OffersService`.

### ⚪ Regeneración de test sin verificar estado de la oferta
`TestService.GenerateTestAsync` con `forceRegenerate: true` no chequea si la oferta está en `TestSent`. Si la empresa regenera el test, se eliminan las preguntas que candidatos tienen activas.

**Solución:** Bloquear `forceRegenerate` si `offer.Status != OfferStatus.Open`.

---

## 🟠 SEGURIDAD

### ⚪ Email del candidato expuesto sin importar el stage en `MatchResultDto`
`MatchingService.cs:402` siempre incluye `Email = match.CandidateProfile.User.Email`. La privacidad (ocultar email hasta `Selected`) solo existe en la función SQL pero el C# la ignora.

**Solución:** En `MapToDto`, devolver `Email = match.Stage == MatchStage.Selected ? match.CandidateProfile.User.Email : null`.

### ⚪ Race condition en `SelectCandidateAsync`
Cuenta candidatos seleccionados antes de guardar → dos requests simultáneos pueden ambos pasar el check y exceder `PositionsAvailable`.

**Solución:** Usar una query atómica con `ExecuteSqlRaw` o mover el cierre de oferta a un UPDATE condicional en SQL.

### ⚪ Código de verificación de email no es criptográficamente seguro
`AuthService.cs:313` usa `Random.Shared.Next()` que es predecible.

**Solución:** Reemplazar con `RandomNumberGenerator.GetInt32(100_000, 1_000_000)`.

---

## 🟡 LÓGICA DE NEGOCIO

### ⚪ Dos sistemas de timeout en paralelo e inconsistentes
- `SendTestsAsync` pone `Deadline = +72 horas`
- `SubmitAnswersAsync` valida `TimeLimitMinutes` desde `StartedAt`
- `DailyJobsService` expira por el `Deadline` (72h)

Un candidato puede ser expirado por el job antes de que venza su `TimeLimitMinutes` si empezó cerca de las 72h.

**Solución:** Definir un solo mecanismo. El recomendado: el `Deadline` se calcula como `StartedAt + TimeLimitMinutes` cuando el candidato hace `StartTest`, no al enviar. El check en `SubmitAnswers` se elimina y se confía en el `Deadline`.

### ⚪ `DailyJobsService` no corre al arrancar el servidor
El `PeriodicTimer` espera 24h antes del primer tick. Submissions/ofertas expiradas durante downtime no se procesan hasta el día siguiente.

**Solución:** Llamar `RunJobsAsync` al inicio de `ExecuteAsync` antes de entrar al loop del timer.

### ⚪ Google login ignora silenciosamente conflicto de rol
Si un usuario ya existe como Candidate y hace login con Google enviando `role: "Company"`, se loguea como Candidate sin ningún aviso.

**Solución:** Si el rol enviado difiere del rol del usuario existente, retornar error claro o al menos incluir un campo `roleConflict` en la respuesta.

---

## 🔵 CALIDAD / ESCALABILIDAD

### ⚪ Sin paginación en endpoints de lista
`GetMyOffersAsync`, `GetMatchesByOfferAsync`, `GetAllUsersAsync` retornan listas completas. Con volumen real es un problema de memoria y latencia.

**Solución:** Agregar parámetros `page` y `pageSize` con un máximo configurable.

### ⚪ N+1 queries en `MatchRepository.RunMatchingAsync`
Por cada candidato de `get_candidate_matches`, se hace una query separada para verificar si ya existe el match. Con 200 candidatos = 200 queries extra.

**Solución:** Cargar todos los matches existentes de la oferta en una sola query antes del loop, y comparar en memoria con un `HashSet<int>`.

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
