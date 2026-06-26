# MatchIQ — Pendientes Backend

> Última actualización: 2026-06-26

---

## Por commitear (código listo, sin commit)

- `POST /api/auth/resend-verification` — reenvío de código de verificación de email.
  Archivos: `ResendVerificationDto.cs`, `AuthService.cs`, `AuthController.cs`

---

## Gaps de seguridad

### 🟠 Google OAuth no valida audience
`GoogleTokenValidator` verifica que el token sea válido con Google, pero no verifica que el `aud` del token coincida con el `Google:ClientId` configurado en `appsettings.json`. Un token generado por cualquier otra app de Google pasaría la validación.

**Solución:** en `GoogleTokenValidator`, al llamar `GoogleJsonWebSignature.ValidateAsync`, pasar `new ValidationSettings { Audience = new[] { clientId } }`.

### 🟠 `RunMatchingAsync` no verifica ownership
`POST /api/matching/{offerId}/run` no valida que la oferta pertenezca a la empresa autenticada. Cualquier empresa con JWT válido puede triggear el matching de una oferta ajena.

**Solución:** en `MatchingService.RunMatchingAsync`, agregar verificación de `offer.CompanyId == company.Id` igual que hacen `ReevaluateAsync` y los demás métodos del mismo servicio.

---

## Gaps de UX / flujo

### 🟡 Ver resumen del test sin iniciarlo (candidato)
Actualmente `GET /api/tests/{offerId}/candidate` registra `StartedAt` en el primer acceso, iniciando el contador de tiempo. El candidato no puede "ver qué le espera" sin consumir su tiempo.

**Solución:** separar en dos endpoints:
- `GET /api/tests/{offerId}/candidate/preview` — devuelve título, tiempo límite, número de preguntas y tipos (sin contenido). No toca `StartedAt`.
- `POST /api/tests/{offerId}/candidate/start` — inicia el test formalmente: registra `StartedAt` y devuelve las preguntas sin respuestas correctas.

Esto requiere un `TestPreviewDto` nuevo. No rompe nada existente.

### 🟡 Email al candidato cuando es seleccionado
Hay email de invitación al test, pero no hay notificación cuando la empresa lo selecciona como finalista. Es el momento más importante del flujo para el candidato.

**Solución:** en `MatchingService.SelectCandidateAsync`, después de guardar, enviar un email al candidato con `IEmailService`. Requiere un nuevo método `SendSelectionNotificationAsync` en la interfaz y su implementación en `MailKitEmailService`.

---

## Gaps de lógica de negocio

### 🟡 Dos condiciones distintas para cerrar una oferta
- `SelectCandidateAsync` en C# cierra la oferta cuando se llenan las `PositionsAvailable`.
- `check_offer_completion()` en SQL cierra cuando todas las submissions quedan `Evaluated` o `Expired`.

Son condiciones distintas y ambas están activas. Pueden entrar en conflicto.

**Decisión pendiente:** definir cuál es la condición real de cierre y desactivar la otra.

### ⚪ `ai_feedback` queda obsoleto al editar la oferta
Si la empresa edita los skills requeridos después de que la IA evaluó candidatos, el análisis cualitativo guardado en `ai_feedback` queda desactualizado aunque el score numérico se recalcule con `ReevaluateAsync`.

**Solución posible:** al editar una oferta (`UpdateOfferAsync`), si cambian `skillIds` o `categoryIds`, borrar el campo `ai_feedback` de todos los matches existentes para forzar reevaluación limpia.

---

## Calidad de código

### 🔴 Sin validaciones en los DTOs
Ningún DTO tiene validaciones. Requests malformados generan 500 en lugar de 400 con mensaje claro. Ejemplos:
- Email con formato inválido en registro
- `selectedOption: "Z"` en submit de test (solo acepta A/B/C/D)
- `level: 999` en skills del candidato (rango válido: 1–5)
- `tierId` inexistente al crear oferta

**Solución:** instalar `FluentValidation.AspNetCore` y crear validators por DTO, o usar `[Required]`, `[EmailAddress]`, `[Range]` de DataAnnotations como mínimo. Registrar en `Program.cs` con `AddFluentValidation()` o activar `ModelState` automático.

---

## Documentación

- Actualizar `API_REFERENCE.md` con los endpoints nuevos:
  - `POST /api/auth/resend-verification`
  - `POST /api/admin/users` (crear admin)
  - `GET /api/tests/{offerId}/candidate/preview` (cuando se implemente)
  - `POST /api/tests/{offerId}/candidate/start` (cuando se implemente)
