# Seleccionar y rechazar candidatos — Guía para frontend

Los botones de "Seleccionar" y "Rechazar" operan sobre un **match** específico, no sobre el
candidato global. Un match es el vínculo entre un candidato y una oferta concreta.

---

## Cuándo mostrar cada botón

Los botones solo tienen sentido en la vista de resultados de una oferta, donde ya se ve el
ranking de candidatos con sus scores y etapas.

| `stage` del match | Botón Seleccionar | Botón Rechazar |
|---|---|---|
| `Matched` | ❌ No disponible | ✅ Mostrar |
| `TestSent` | ❌ No disponible | ✅ Mostrar |
| `TestCompleted` | ✅ Mostrar | ✅ Mostrar |
| `Selected` | ❌ Ocultar | ❌ Ocultar |
| `Rejected` | ❌ Ocultar | ❌ Ocultar |

**Regla clave:** el botón Seleccionar solo aparece si `stage === "TestCompleted"`. El backend
lo rechazará con 400 si el candidato no está en ese estado exacto.

---

## Endpoint: Seleccionar candidato

```
POST /api/matching/{matchId}/select
Authorization: Bearer <token>   (rol: Company)
Sin body
```

### Respuesta exitosa `200`

```json
{
  "success": true,
  "data": {
    "matchId": 42,
    "candidateId": 7,
    "fullName": "Laura Gómez",
    "email": "laura@email.com",
    "experienceYears": 4,
    "englishLevel": "B2",
    "matchPercentage": 87.5,
    "adjustedScore": 91.2,
    "stage": "Selected",
    "aiInsight": "Candidata con alto dominio técnico y buen encaje cultural.",
    "aiStrengths": ["React avanzado", "Experiencia en arquitectura limpia"],
    "aiOpportunities": ["Poca experiencia con DevOps"],
    "aiRecommendation": "Recomendada para el cargo.",
    "matchedSkills": ["React", "TypeScript", "Node.js"],
    "createdAt": "2026-06-20T14:30:00Z",
    "testScore": 88.5,
    "testFeedback": "Excelente resolución del code challenge..."
  },
  "message": null
}
```

El candidato recibe un **email de selección** de forma automática (best-effort — si falla el
envío del correo, la selección ya quedó guardada de todas formas).

### Efecto secundario importante

Si con esta selección se completan todas las posiciones disponibles de la oferta
(`positionsAvailable`), **la oferta pasa automáticamente a estado `Completed`**. El frontend
debe reflejar esto al recargar la oferta después de la acción.

### Errores posibles

| HTTP | `message` | Qué hacer en UI |
|---|---|---|
| `400` | `"Solo puedes seleccionar candidatos que hayan completado el test."` | El botón no debería haber aparecido — verificar la lógica de `stage` |
| `401` | — | Token expirado, redirigir a login |
| `403` | `"No tienes acceso a este match."` | La oferta no pertenece a esta empresa |
| `404` | `"Match no encontrado."` | El `matchId` es inválido |

---

## Endpoint: Rechazar candidato

```
POST /api/matching/{matchId}/reject
Authorization: Bearer <token>   (rol: Company)
Sin body
```

### Respuesta exitosa `200`

```json
{
  "success": true,
  "data": null,
  "message": "Candidato rechazado correctamente."
}
```

El candidato recibe un **email de rechazo** automáticamente (best-effort).

### Errores posibles

| HTTP | `message` | Qué hacer en UI |
|---|---|---|
| `400` | `"No se puede rechazar un candidato que ya fue seleccionado."` | El botón no debería haber aparecido |
| `400` | `"El candidato ya fue rechazado."` | Idem |
| `401` | — | Token expirado |
| `403` | `"No tienes acceso a este match."` | Oferta de otra empresa |
| `404` | `"Match no encontrado."` | `matchId` inválido |

---

## Flujo de implementación en Flutter

```
Vista: lista de candidatos de una oferta
       │
       ├── Para cada candidato en la lista:
       │     ├── stage == "TestCompleted"
       │     │     ├── Botón "Seleccionar" (verde)
       │     │     └── Botón "Rechazar"   (rojo)
       │     │
       │     ├── stage == "Matched" || "TestSent"
       │     │     └── Solo botón "Rechazar"
       │     │
       │     └── stage == "Selected" || "Rejected"
       │           └── Badge de estado (sin botones)
       │
       ├── Usuario toca "Seleccionar":
       │     ├── Mostrar diálogo de confirmación:
       │     │     "¿Confirmas la selección de [nombre]?
       │     │      Se le notificará por correo."
       │     ├── Si confirma → POST /api/matching/{matchId}/select
       │     ├── Si success:
       │     │     ├── Actualizar stage del candidato en la lista → "Selected"
       │     │     ├── Recargar el detalle de la oferta (puede haber pasado a Completed)
       │     │     └── Mostrar snackbar "Candidato seleccionado correctamente."
       │     └── Si error → Mostrar message del response
       │
       └── Usuario toca "Rechazar":
             ├── Mostrar diálogo de confirmación:
             │     "¿Confirmas el rechazo de [nombre]?
             │      Esta acción no se puede deshacer."
             ├── Si confirma → POST /api/matching/{matchId}/reject
             ├── Si success:
             │     ├── Actualizar stage del candidato en la lista → "Rejected"
             │     └── Mostrar snackbar "Candidato rechazado."
             └── Si error → Mostrar message del response
```

### Actualización optimista vs recarga

Se recomienda **actualización optimista**: al recibir `success: true`, actualizar el `stage`
del candidato localmente en el estado del widget sin rehacer el `GET` del ranking completo.
Solo recargar la oferta completa para verificar si cambió a `Completed`.

---

## Campos del candidato en la lista (`MatchResultDto`)

Estos son todos los campos que devuelve el `GET /api/matching/{offerId}` para cada candidato,
y que el `POST select` también devuelve en su `data`:

| Campo | Tipo | Descripción |
|---|---|---|
| `matchId` | `int` | ID del match — es el que se usa en los endpoints de selección/rechazo |
| `candidateId` | `int` | ID del perfil de candidato |
| `fullName` | `string` | Nombre completo |
| `email` | `string?` | Email (visible siempre para la empresa en este contexto) |
| `experienceYears` | `int?` | Años de experiencia declarados |
| `englishLevel` | `string?` | Nivel de inglés: `A1`–`C2` |
| `matchPercentage` | `decimal?` | Score del algoritmo SQL (0–100) |
| `adjustedScore` | `decimal?` | Score final: 90% SQL + 10% IA (0–100). `null` si aún no fue evaluado por IA |
| `stage` | `string` | Estado actual del match: `Matched` · `TestSent` · `TestCompleted` · `Selected` · `Rejected` |
| `aiInsight` | `string?` | Resumen cualitativo de la IA sobre el candidato |
| `aiStrengths` | `string[]` | Lista de fortalezas identificadas por la IA |
| `aiOpportunities` | `string[]` | Lista de áreas de mejora identificadas por la IA |
| `aiRecommendation` | `string?` | Recomendación final de la IA |
| `matchedSkills` | `string[]` | Skills del candidato que coinciden con los requeridos por la oferta |
| `testScore` | `decimal?` | Puntaje del test técnico (0–100). `null` si no completó el test |
| `testFeedback` | `string?` | Feedback de la IA sobre las respuestas del test |
| `createdAt` | `datetime` | Fecha en que el candidato apareció en el ranking |
