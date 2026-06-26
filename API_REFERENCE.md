# MatchIQ — Referencia de API para Frontend

## Información general

| | |
|---|---|
| **Base URL (dev)** | `http://localhost:5000` |
| **Formato** | JSON (`Content-Type: application/json`) |
| **Autenticación** | `Authorization: Bearer {accessToken}` |

---

## Formato de respuesta (SIEMPRE el mismo)

Todos los endpoints devuelven este contrato sin excepción:

```json
{
  "success": true,
  "data": { },
  "message": null
}
```

```json
{
  "success": false,
  "data": null,
  "message": "Descripción del error"
}
```

**Regla en Flutter:** checa `success` primero. Si es `true`, usa `data`. Si es `false`, muestra `message` al usuario.

### Códigos HTTP que puede devolver

| Código | Cuándo |
|---|---|
| `200` | Éxito |
| `400` | Error de negocio (input inválido, estado incorrecto, etc.) |
| `401` | Sin token, token expirado, o sin permiso de rol |
| `404` | Recurso no encontrado |
| `429` | Rate limit excedido |
| `500` | Error interno del servidor |

---

## Valores válidos para campos de tipo enum

Estos son los únicos valores aceptados en los campos de texto que representan enums:

| Campo | Valores válidos |
|---|---|
| `role` (al registrar) | `0` = Admin, `1` = Candidate, `2` = Company |
| `modality` | `"remote"`, `"onsite"`, `"hybrid"` |
| `englishLevel` | `"A1"`, `"A2"`, `"B1"`, `"B2"`, `"C1"`, `"C2"` |
| `seniority` | `"junior"`, `"mid"`, `"senior"` |
| `offerStatus` | `"PendingPayment"`, `"Open"`, `"TestSent"`, `"Completed"`, `"Cancelled"`, `"Expired"` |
| `matchStage` | `"Matched"`, `"TestSent"`, `"TestCompleted"`, `"Selected"`, `"Rejected"` |
| `submissionStatus` | `"Pending"`, `"Evaluated"`, `"Expired"` |
| `questionType` | `"MultipleChoice"`, `"CodeChallenge"` |

---

## Leyenda de autenticación

- 🔓 **Público** — no requiere token
- 🔐 **JWT** — requiere `Authorization: Bearer {token}` (cualquier rol)
- 👤 **Candidate** — solo candidatos
- 🏢 **Company** — solo empresas
- 🛡️ **Admin** — solo administradores

---

---

# MÓDULO: AUTH

Base path: `/api/auth`

---

### 1. Registrar usuario
`POST /api/auth/register` · 🔓 Público

**Body:**
```json
{
  "fullName": "Juan Pérez",
  "email": "juan@email.com",
  "cedula": "1234567890",
  "password": "MiPassword123",
  "confirmPassword": "MiPassword123",
  "role": 1
}
```
> `role`: `1` = Candidate, `2` = Company

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": null,
  "message": "Registro exitoso. Revisa tu email e ingresa el código de verificación."
}
```

---

### 2. Verificar email
`POST /api/auth/verify-email` · 🔓 Público

**Body:**
```json
{
  "email": "juan@email.com",
  "code": "482910"
}
```
> El código llega al email del usuario. Expira en 10 minutos.

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": null,
  "message": "Email verificado. Ya puedes iniciar sesión y completar tu perfil."
}
```

---

### 3. Iniciar sesión
`POST /api/auth/login` · 🔓 Público

**Body:**
```json
{
  "email": "juan@email.com",
  "password": "MiPassword123"
}
```

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGci...",
    "refreshToken": "d3f8a2...",
    "userId": 5,
    "role": "Candidate",
    "fullName": "Juan Pérez",
    "emailVerified": true
  },
  "message": null
}
```
> Guarda `accessToken` y `refreshToken` en almacenamiento seguro. El `accessToken` dura **15 minutos**. El `refreshToken` dura **7 días**.

---

### 4. Renovar token
`POST /api/auth/refresh` · 🔓 Público

**Body:**
```json
{
  "refreshToken": "d3f8a2..."
}
```

**Respuesta exitosa:** igual a login — devuelve nuevos `accessToken` y `refreshToken`.

---

### 5. Login con Google
`POST /api/auth/google` · 🔓 Público

**Body:**
```json
{
  "idToken": "token_que_google_devuelve_al_frontend",
  "role": 1
}
```
> `role` solo aplica si el usuario es nuevo. Para usuarios existentes se ignora.

**Respuesta exitosa:** igual a login.

---

### 6. Olvidé mi contraseña
`POST /api/auth/forgot-password` · 🔓 Público

**Body:**
```json
{
  "email": "juan@email.com"
}
```

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": null,
  "message": "Si el email existe, recibirás un enlace para restablecer tu contraseña."
}
```

---

### 7. Restablecer contraseña
`POST /api/auth/reset-password` · 🔓 Público

**Body:**
```json
{
  "token": "token_del_link_del_email",
  "newPassword": "NuevoPassword123",
  "confirmPassword": "NuevoPassword123"
}
```

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": null,
  "message": "Contraseña actualizada correctamente."
}
```

---

### 8. Cerrar sesión
`POST /api/auth/logout` · 🔐 JWT

**Body:**
```json
{
  "refreshToken": "d3f8a2..."
}
```

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": null,
  "message": "Sesión cerrada."
}
```

---

---

# MÓDULO: CATÁLOGO

Base path: `/api/catalog`
> Cargar esto al iniciar la app. Se usa para poblar los selectores de categorías y skills.

---

### 9. Listar categorías
`GET /api/catalog/categories` · 🔓 Público

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": [
    { "id": 1, "name": "FrontEnd" },
    { "id": 2, "name": "BackEnd" },
    { "id": 3, "name": "FullStack" },
    { "id": 4, "name": "DevOps" },
    { "id": 5, "name": "QA" },
    { "id": 6, "name": "UX/UI" },
    { "id": 7, "name": "Databases" }
  ],
  "message": null
}
```

---

### 10. Skills por categoría
`GET /api/catalog/categories/{categoryId}/skills` · 🔓 Público

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": [
    { "id": 1, "name": "React", "categoryId": 1 },
    { "id": 2, "name": "Vue.js", "categoryId": 1 }
  ],
  "message": null
}
```

---

---

# MÓDULO: CANDIDATO

Base path: `/api/candidate`

---

### 11. Ver mi perfil
`GET /api/candidate/profile` · 👤 Candidate

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": {
    "userId": 5,
    "fullName": "Juan Pérez",
    "email": "juan@email.com",
    "experienceYears": 3,
    "seniority": "mid",
    "englishLevel": "B2",
    "githubLink": "https://github.com/juan",
    "linkedinUrl": "https://linkedin.com/in/juan",
    "profilePhotoUrl": "https://storage.example.com/foto.jpg",
    "profileCompleted": true,
    "categories": [
      { "id": 1, "name": "FrontEnd" }
    ],
    "skills": [
      {
        "skillId": 1,
        "skillName": "React",
        "categoryId": 1,
        "categoryName": "FrontEnd",
        "level": 4
      }
    ]
  },
  "message": null
}
```

---

### 12. Actualizar mi perfil
`PUT /api/candidate/profile` · 👤 Candidate

**Body:**
```json
{
  "experienceYears": 3,
  "seniority": "mid",
  "englishLevel": "B2",
  "githubLink": "https://github.com/juan",
  "linkedinUrl": "https://linkedin.com/in/juan",
  "profilePhotoUrl": "https://storage.example.com/foto.jpg",
  "categoryIds": [1, 2],
  "skills": [
    { "skillId": 1, "level": 4 },
    { "skillId": 9, "level": 3 }
  ]
}
```
> Todos los campos son opcionales. `level` va de **1 a 5**.
> Al actualizar el perfil, el sistema re-evalúa automáticamente el candidato contra todas las ofertas abiertas.

**Respuesta exitosa:** devuelve el perfil completo igual que GET.

---

---

# MÓDULO: EMPRESA

Base path: `/api/company`

---

### 13. Ver mi perfil de empresa
`GET /api/company/profile` · 🏢 Company

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": {
    "userId": 10,
    "fullName": "Ana García",
    "email": "ana@empresa.com",
    "companyName": "Tech Solutions SAS",
    "profileCompleted": true,
    "createdAt": "2026-06-01T10:00:00Z"
  },
  "message": null
}
```

---

### 14. Actualizar perfil de empresa
`PUT /api/company/profile` · 🏢 Company

**Body:**
```json
{
  "companyName": "Tech Solutions SAS"
}
```

**Respuesta exitosa:** devuelve el perfil completo.

---

---

# MÓDULO: OFERTAS

Base path: `/api/offers`

---

### 15. Ver tiers de precio
`GET /api/offers/tiers` · 🏢 Company

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": [
    { "id": 1, "name": "Starter",  "minCandidates": 1, "maxCandidates": 1,  "priceCop": 89000 },
    { "id": 2, "name": "Básico",   "minCandidates": 2, "maxCandidates": 3,  "priceCop": 199000 },
    { "id": 3, "name": "Estándar", "minCandidates": 4, "maxCandidates": 7,  "priceCop": 349000 },
    { "id": 4, "name": "Avanzado", "minCandidates": 8, "maxCandidates": 15, "priceCop": 599000 }
  ],
  "message": null
}
```

---

### 16. Parsear descripción con IA (paso opcional antes de crear)
`POST /api/offers/parse-description` · 🏢 Company

La empresa pega una descripción libre del cargo y la IA extrae los campos. Útil para pre-llenar el formulario de creación.

**Body:**
```json
{
  "rawDescription": "Buscamos un desarrollador React con 2 años de experiencia, inglés B2, trabajo remoto..."
}
```

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": {
    "title": "Desarrollador React",
    "modality": "remote",
    "salary": null,
    "minExperienceYears": 2,
    "requiredEnglishLevel": "B2",
    "suggestedCategoryIds": [1],
    "suggestedSkillIds": [4],
    "confidenceNote": "Se identificaron skills con alta confianza."
  },
  "message": null
}
```
> Muestra los campos pre-llenados y editables. El usuario confirma antes de crear la oferta.

---

### 17. Crear oferta
`POST /api/offers` · 🏢 Company

**Body:**
```json
{
  "title": "Desarrollador React Senior",
  "description": "Descripción detallada del cargo...",
  "salary": 5000000,
  "modality": "remote",
  "minExperienceYears": 3,
  "requiredEnglishLevel": "B2",
  "positionsAvailable": 2,
  "tierId": 3,
  "categoryIds": [1, 2],
  "skillIds": [4, 9, 10]
}
```
> La oferta se crea en estado `PendingPayment`. Para activarla, la empresa debe pagar (ver `/api/payments/create-checkout`).

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": {
    "id": 7,
    "title": "Desarrollador React Senior",
    "description": "...",
    "salary": 5000000,
    "modality": "remote",
    "minExperienceYears": 3,
    "requiredEnglishLevel": "B2",
    "positionsAvailable": 2,
    "tierId": 3,
    "tierName": "Estándar",
    "tierPriceCop": 349000,
    "candidatesToTest": null,
    "status": "PendingPayment",
    "createdAt": "2026-06-25T14:00:00Z",
    "paidAt": null,
    "expiresAt": null,
    "categories": [ { "id": 1, "name": "FrontEnd" } ],
    "skills": [ { "id": 4, "name": "React", "categoryId": 1 } ],
    "checkoutUrl": null
  },
  "message": "Oferta creada correctamente."
}
```

---

### 18. Ver mis ofertas
`GET /api/offers` · 🏢 Company

**Respuesta exitosa:** `data` es una lista de objetos con la misma forma que en "Crear oferta".

---

### 19. Ver una oferta
`GET /api/offers/{id}` · 🏢 Company

**Respuesta exitosa:** `data` es el objeto de la oferta.

---

### 20. Editar oferta
`PUT /api/offers/{id}` · 🏢 Company

Solo se pueden editar ofertas en estado `PendingPayment` o `Open`.

**Body:** todos los campos son opcionales:
```json
{
  "title": "Nuevo título",
  "description": "Nueva descripción",
  "salary": 6000000,
  "modality": "hybrid",
  "minExperienceYears": 2,
  "requiredEnglishLevel": "B1",
  "positionsAvailable": 1
}
```

**Respuesta exitosa:** devuelve la oferta actualizada.

---

### 21. Cancelar oferta
`PATCH /api/offers/{id}/cancel` · 🏢 Company

**Sin body.**

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": {
    "cancelled": true,
    "warning": "Hay 2 candidatos en proceso de test. Al cancelar, sus submissions quedarán sin evaluar.",
    "candidatesInProgress": 2
  },
  "message": "Oferta cancelada correctamente."
}
```
> Si `warning` no es null, muestra una confirmación al usuario antes de llamar este endpoint.

---

### 22. Forzar cancelación
`POST /api/offers/{id}/force-cancel` · 🏢 Company

Cancela aunque haya candidatos en proceso. Llamar solo si el usuario confirmó la advertencia del endpoint anterior.

**Sin body.**

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": null,
  "message": "Oferta forzada a cancelar correctamente."
}
```

---

---

# MÓDULO: PAGOS

Base path: `/api/payments`

---

### 23. Crear link de pago
`POST /api/payments/create-checkout?offerId=7` · 🏢 Company

La oferta debe estar en estado `PendingPayment`. Si ya se generó un link antes, devuelve el mismo (idempotente).

**Sin body** — el `offerId` va en el query string.

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": {
    "url": "https://checkout.wompi.co/l/..."
  },
  "message": null
}
```
> Redirige al usuario a esa URL. Wompi se encarga del resto. Al pagar, el webhook activa la oferta automáticamente.

---

### 24. Webhook de Wompi
`POST /api/payments/webhook` · 🔓 Público (solo para Wompi)

> **No llamar desde el frontend.** Este endpoint es exclusivo para que Wompi notifique al backend cuando un pago es aprobado. Lo configuras en el dashboard de Wompi.

---

---

# MÓDULO: MATCHING

Base path: `/api/matching`

---

### 25. Ver ranking de candidatos
`GET /api/matching/{offerId}` · 🏢 Company

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": [
    {
      "matchId": 12,
      "candidateId": 5,
      "fullName": "Juan Pérez",
      "email": "juan@email.com",
      "experienceYears": 3,
      "englishLevel": "B2",
      "matchPercentage": 87.50,
      "adjustedScore": 91.20,
      "stage": "Matched",
      "aiInsight": "Candidato con sólido dominio de React y experiencia alineada.",
      "aiStrengths": ["React avanzado", "Experiencia en proyectos escalables"],
      "aiOpportunities": ["Puede reforzar TypeScript"],
      "aiRecommendation": "Recomendado para el test técnico.",
      "matchedSkills": ["React", "JavaScript"],
      "createdAt": "2026-06-25T15:00:00Z"
    }
  ],
  "message": null
}
```
> La lista viene ordenada por `adjustedScore` desc (o `matchPercentage` si aún no tiene score de IA).
> `aiInsight`, `aiStrengths`, `aiOpportunities`, `aiRecommendation` pueden ser `null` para candidatos que aún no fueron evaluados por IA (solo el top 3 recibe evaluación automática).

---

### 26. Ejecutar matching manualmente
`POST /api/matching/{offerId}/run` · 🏢 Company

Corre el matching incremental (solo candidatos nuevos). Normalmente el sistema lo hace automático; este endpoint es para forzarlo.

**Sin body.**

**Respuesta exitosa:** lista de matches igual que en "Ver ranking".

---

### 27. Reevaluar ranking completo
`POST /api/matching/{offerId}/reevaluate` · 🏢 Company

Recalcula el score de TODOS los candidatos, incluyendo los que ya tenían match. Útil cuando la oferta fue editada.

**Sin body.**

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": [ /* lista completa de matches actualizada */ ],
  "message": "Reevaluación completada."
}
```

---

### 28. Enviar test a candidatos
`POST /api/matching/send-test` · 🏢 Company

Evento único — no se puede volver a enviar. Los candidatos reciben el test por email con un link directo.

**Body:**
```json
{
  "matchIds": [12, 15, 18]
}
```
> `matchIds`: IDs de los matches (no de los candidatos). Solo se pueden enviar candidatos en stage `Matched`.

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": null,
  "message": "Tests enviados correctamente. Los candidatos recibirán un correo con el enlace."
}
```

---

### 29. Seleccionar candidato
`POST /api/matching/{matchId}/select` · 🏢 Company

Solo se puede seleccionar desde stage `TestCompleted`.

**Sin body.**

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": { /* objeto del match actualizado */ },
  "message": "Candidato seleccionado correctamente."
}
```

---

### 30. Rechazar candidato
`POST /api/matching/{matchId}/reject` · 🏢 Company

Se puede rechazar desde `Matched`, `TestSent` o `TestCompleted`. No desde `Selected`.

**Sin body.**

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": null,
  "message": "Candidato rechazado correctamente."
}
```

---

---

# MÓDULO: TESTS

Base path: `/api/tests`

---

### 31. Generar test con IA
`POST /api/tests/{offerId}/generate` · 🏢 Company

Genera el test técnico para la oferta. Se hace una sola vez al crear la oferta.

**Sin body.**

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": {
    "id": 3,
    "offerId": 7,
    "title": "Test técnico — Desarrollador React Senior",
    "timeLimitMinutes": 45,
    "createdAt": "2026-06-25T14:30:00Z",
    "questions": [
      {
        "id": 10,
        "orderIndex": 1,
        "questionType": "CodeChallenge",
        "questionText": "Implementa una función que...",
        "isGorilla": false,
        "gorillaHint": null,
        "correctAnswer": null,
        "explanation": null,
        "options": null,
        "language": "javascript",
        "functionSignature": "function solve(arr) { }",
        "exampleInput": "[1, 2, 3]",
        "expectedBehavior": "Retorna el máximo valor"
      },
      {
        "id": 11,
        "orderIndex": 2,
        "questionType": "MultipleChoice",
        "questionText": "¿Cuál es la diferencia entre == y === en JavaScript?",
        "isGorilla": false,
        "gorillaHint": null,
        "correctAnswer": "B",
        "explanation": "=== compara valor Y tipo...",
        "options": {
          "A": "No hay diferencia",
          "B": "=== compara tipo además del valor",
          "C": "== es más estricto",
          "D": "Solo se usa == en JavaScript moderno"
        },
        "language": null,
        "functionSignature": null,
        "exampleInput": null,
        "expectedBehavior": null
      }
    ]
  },
  "message": "Test generado correctamente."
}
```

---

### 32. Regenerar test
`POST /api/tests/{offerId}/regenerate` · 🏢 Company

Reemplaza el test existente con uno nuevo generado por IA.

**Sin body.** Respuesta igual que generar.

---

### 33. Ver test completo (empresa)
`GET /api/tests/{offerId}` · 🏢 Company

Devuelve el test con todas las respuestas correctas visibles. Respuesta igual que generar.

---

### 34. Ver historial de chat de una pregunta
`GET /api/tests/questions/{questionId}/chat` · 🏢 Company

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": [
    { "role": "admin", "content": "Cambia el nivel a más difícil", "createdAt": "2026-06-25T15:00:00Z" },
    { "role": "assistant", "content": "Entendido, aquí la nueva versión...", "createdAt": "2026-06-25T15:00:05Z" }
  ],
  "message": null
}
```

---

### 35. Editar pregunta con IA (chat)
`POST /api/tests/questions/{questionId}/chat` · 🏢 Company

Envía un mensaje al asistente para modificar esa pregunta específica.

**Body:**
```json
{
  "message": "Hazla más difícil y enfócala en hooks de React"
}
```

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": {
    "updatedQuestion": { /* objeto QuestionDto con la pregunta actualizada */ },
    "assistantMessage": "Actualicé la pregunta para incluir useEffect y useCallback..."
  },
  "message": null
}
```

---

### 36. Ver test (candidato)
`GET /api/tests/{offerId}/candidate` · 👤 Candidate

Devuelve el test SIN respuestas correctas ni hints. La primera llamada registra `startedAt` e inicia el contador de tiempo.

**Respuesta exitosa:** igual que generar test, pero los campos `correctAnswer`, `explanation`, `isGorilla` y `gorillaHint` siempre vienen `null`.

---

### 37. Enviar respuestas
`POST /api/tests/{testId}/submit` · 👤 Candidate

**Body:**
```json
{
  "answers": [
    {
      "questionId": 10,
      "selectedOption": null,
      "codeSubmitted": "function solve(arr) { return Math.max(...arr); }"
    },
    {
      "questionId": 11,
      "selectedOption": "B",
      "codeSubmitted": null
    }
  ]
}
```
> Para `MultipleChoice`: llenar `selectedOption` con `"A"`, `"B"`, `"C"` o `"D"`.
> Para `CodeChallenge`: llenar `codeSubmitted` con el código escrito por el candidato.
> Si el tiempo límite expiró desde que inició el test, la respuesta será `400` con un mensaje de tiempo agotado.

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": {
    "score": 82.50,
    "feedback": "Buen desempeño general. El código del challenge es funcional aunque podría optimizarse.",
    "status": "Evaluated",
    "submittedAt": "2026-06-25T16:00:00Z",
    "aiEvaluatedAt": "2026-06-25T16:00:10Z",
    "questionResults": [
      { "questionId": 10, "isCorrect": true, "feedback": "Solución correcta y eficiente." },
      { "questionId": 11, "isCorrect": true, "feedback": null }
    ]
  },
  "message": "Respuestas enviadas y evaluadas correctamente."
}
```

---

### 38. Ver resultado de un test
`GET /api/tests/{testId}/result` · 👤 Candidate

Devuelve el resultado si ya fue evaluado. Respuesta igual que enviar respuestas.

---

---

# MÓDULO: ADMIN

Base path: `/api/admin`

---

### 39. Listar usuarios
`GET /api/admin/users` · 🛡️ Admin

**Query params opcionales:**
- `role`: `"Candidate"`, `"Company"` o `"Admin"`
- `isActive`: `true` o `false`

Ejemplo: `GET /api/admin/users?role=Company&isActive=true`

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": [
    {
      "id": 5,
      "email": "juan@email.com",
      "fullName": "Juan Pérez",
      "cedula": "1234567890",
      "role": "Candidate",
      "isActive": true,
      "emailVerified": true,
      "createdAt": "2026-06-01T10:00:00Z",
      "profileName": null
    }
  ],
  "message": null
}
```
> `profileName`: nombre de empresa para usuarios Company, `null` para candidatos.

---

### 40. Ver usuario por ID
`GET /api/admin/users/{userId}` · 🛡️ Admin

**Respuesta exitosa:** objeto igual al de la lista.

---

### 41. Activar / desactivar cuenta
`PATCH /api/admin/users/{userId}/toggle-status` · 🛡️ Admin

**Sin body.**

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": { /* objeto del usuario con el nuevo estado */ },
  "message": "Cuenta desactivada correctamente."
}
```

---

### 42. Eliminar usuario
`DELETE /api/admin/users/{userId}` · 🛡️ Admin

Elimina en cascada: perfil, offers, matches, submissions, etc.

**Sin body.**

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": null,
  "message": "Usuario eliminado correctamente."
}
```

---

### 43. Estadísticas del sistema
`GET /api/admin/stats` · 🛡️ Admin

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": {
    "totalCandidates": 120,
    "totalCompanies": 35,
    "totalOffers": 58,
    "totalMatches": 430,
    "activeTests": 12,
    "pendingSubmissions": 8,
    "offersByStatus": {
      "PendingPayment": 5,
      "Open": 18,
      "TestSent": 12,
      "Completed": 20,
      "Cancelled": 2,
      "Expired": 1
    },
    "usersRegisteredLast30Days": 45,
    "offersCreatedLast30Days": 10
  },
  "message": null
}
```

---

---

## Flujo típico por rol

### Flujo Empresa (Company)
```
1. register → 2. verify-email → 3. login
4. company/profile (PUT) — completar nombre de empresa
5. catalog/categories + catalog/categories/{id}/skills — cargar catálogo
6. offers/tiers — mostrar precios
7. offers/parse-description (opcional) — pre-llenar formulario con IA
8. offers (POST) — crear oferta
9. payments/create-checkout?offerId=X — generar link de Wompi y redirigir
   [Wompi procesa el pago y llama al webhook — la oferta pasa a Open automáticamente]
10. tests/{offerId}/generate — generar test técnico
11. tests/questions/{id}/chat (POST) — editar preguntas (opcional)
12. matching/{offerId} (GET) — ver ranking de candidatos
13. matching/send-test (POST) — enviar test a candidatos seleccionados
14. matching/{offerId} (GET) — ver resultados cuando candidates completen el test
15. matching/{matchId}/select o /reject — tomar decisión final
```

### Flujo Candidato (Candidate)
```
1. register → 2. verify-email → 3. login
4. candidate/profile (PUT) — completar perfil (dispara matching automático)
   [El sistema evalúa al candidato contra todas las ofertas abiertas]
   [Candidato recibe email si es seleccionado para presentar un test]
5. tests/{offerId}/candidate (GET) — ver el test (inicia el contador de tiempo)
6. tests/{testId}/submit (POST) — enviar respuestas antes de que expire el tiempo
7. tests/{testId}/result (GET) — ver calificación y feedback
```

---

## Notas importantes para implementación

1. **Manejo del token:** al recibir `401`, intentar renovar con `/api/auth/refresh`. Si el refresh también falla, redirigir al login.
2. **Tiempo del test:** al llamar `GET /tests/{offerId}/candidate`, guardar `timeLimitMinutes`. Mostrar un contador regresivo en la UI. Si el tiempo corre, el backend rechaza el submit con `400`.
3. **Estado de oferta:** mostrar `checkoutUrl` en la oferta cuando `status = "PendingPayment"` para que la empresa pueda pagar.
4. **Privacidad en matching:** el email del candidato solo aparece real cuando `stage = "Selected"`. En otros stages puede mostrarse con máscara si el backend lo retorna.
5. **Rate limits:** no reintentar inmediatamente al recibir `429` — esperar al menos 60 segundos.
6. **Catálogo:** cargar categorías y skills una sola vez al inicio y cachearlas localmente — no cambian con frecuencia.
