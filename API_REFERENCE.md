# MatchIQ — Referencia de API para Frontend

## Información general

| | |
|---|---|
| **Base URL (dev)** | `http://localhost:5000` |
| **Formato** | JSON (`Content-Type: application/json`) |
| **Autenticación** | `Authorization: Bearer {accessToken}` |

---

## Formato de respuesta (SIEMPRE el mismo)

Todos los endpoints devuelven este contrato sin excepción — incluyendo errores de validación:

```json
{ "success": true,  "data": { },   "message": null }
{ "success": true,  "data": null,  "message": "Operación exitosa." }
{ "success": false, "data": null,  "message": "Descripción del error." }
```

**Regla en Flutter:** checa `success` primero. Si es `true`, usa `data` (o `message` si `data` es null). Si es `false`, muestra `message` al usuario.

### Códigos HTTP posibles

| Código | Cuándo |
|---|---|
| `200` | Éxito |
| `400` | Input inválido, regla de negocio violada, estado incorrecto |
| `401` | Sin token, token expirado, o sin permiso de rol |
| `404` | Recurso no encontrado |
| `429` | Rate limit excedido — esperar al menos 60 segundos antes de reintentar |
| `500` | Error interno del servidor |

---

## Valores válidos para campos enum

| Campo | Valores aceptados |
|---|---|
| `role` (al registrar / Google login) | `1` = Candidate · `2` = Company |
| `modality` | `"remote"` · `"onsite"` · `"hybrid"` |
| `englishLevel` / `requiredEnglishLevel` | `"A1"` · `"A2"` · `"B1"` · `"B2"` · `"C1"` · `"C2"` |
| `seniority` | `"junior"` · `"mid"` · `"senior"` |
| `selectedOption` (respuesta test) | `"A"` · `"B"` · `"C"` · `"D"` |

> Los enums en las **respuestas** vienen como strings: `"stage": "Matched"`, `"status": "Open"`, etc.

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
`POST /api/auth/register` · 🔓 Público · ⚡ 5 req/min por IP

**Body:**
```json
{
  "fullName": "Juan Pérez",
  "email": "juan@email.com",
  "cedula": "1234567890",
  "password": "MiPass.123",
  "confirmPassword": "MiPass.123",
  "role": 1
}
```

| Campo | Reglas |
|---|---|
| `fullName` | Requerido |
| `email` | Requerido · formato email válido |
| `cedula` | Requerido |
| `password` | Requerido · mínimo 8 caracteres · debe incluir al menos una mayúscula, una minúscula, un número y un carácter especial (`.` cuenta) |
| `confirmPassword` | Requerido · debe ser idéntico a `password` |
| `role` | `1` (Candidate) o `2` (Company) — no se acepta `0` (Admin) |

**Respuesta exitosa:**
```json
{ "success": true, "data": null, "message": "Registro exitoso. Revisa tu email e ingresa el código de verificación." }
```

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"Las contraseñas no coinciden."` |
| 400 | `"La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial."` |
| 400 | `"El campo Email no es una dirección de correo electrónico válida."` |
| 400 | `"El email ya está registrado."` |
| 400 | `"La cédula ya está registrada."` |
| 400 | `"No se puede registrar un administrador."` |

---

### 2. Verificar email
`POST /api/auth/verify-email` · 🔓 Público · ⚡ 15 req/min por IP

**Body:**
```json
{
  "email": "juan@email.com",
  "code": "482910"
}
```

| Campo | Reglas |
|---|---|
| `email` | Requerido · formato email válido |
| `code` | Requerido · exactamente 6 dígitos numéricos · expira en 10 minutos |

**Respuesta exitosa:**
```json
{ "success": true, "data": null, "message": "Email verificado. Ya puedes iniciar sesión y completar tu perfil." }
```

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"El código debe ser de 6 dígitos."` |
| 400 | `"Este email ya fue verificado. Puedes iniciar sesión."` |
| 400 | `"Código inválido o expirado."` |
| 404 | `"Usuario no encontrado."` |

---

### 3. Reenviar código de verificación
`POST /api/auth/resend-verification` · 🔓 Público · ⚡ 5 req/min por IP

Genera un nuevo código de 6 dígitos e invalida el anterior. Responde igual aunque el email no exista (por seguridad).

**Body:**
```json
{
  "email": "juan@email.com"
}
```

| Campo | Reglas |
|---|---|
| `email` | Requerido · formato email válido |

**Respuesta exitosa:**
```json
{ "success": true, "data": null, "message": "Si el email existe y no está verificado, recibirás un nuevo código." }
```

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"El campo Email no es una dirección de correo electrónico válida."` |

---

### 4. Iniciar sesión
`POST /api/auth/login` · 🔓 Público · ⚡ 5 req/min por IP

**Body:**
```json
{
  "email": "juan@email.com",
  "password": "MiPass.123"
}
```

| Campo | Reglas |
|---|---|
| `email` | Requerido · formato email válido |
| `password` | Requerido |

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
> `accessToken` dura **60 minutos**. `refreshToken` dura **7 días**. Guardar ambos en almacenamiento seguro.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"Credenciales inválidas."` |
| 400 | `"Debes verificar tu email antes de iniciar sesión."` |
| 400 | `"Tu cuenta está desactivada. Contacta al soporte."` |

---

### 5. Renovar token
`POST /api/auth/refresh` · 🔓 Público · ⚡ 15 req/min por IP

**Body:**
```json
{ "refreshToken": "d3f8a2..." }
```

| Campo | Reglas |
|---|---|
| `refreshToken` | Requerido |

**Respuesta exitosa:** igual a login — devuelve nuevos `accessToken` y `refreshToken`.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"Refresh token inválido o expirado."` |
| 400 | `"Tu cuenta está desactivada."` |

---

### 6. Login con Google
`POST /api/auth/google` · 🔓 Público · ⚡ 15 req/min por IP

**Body:**
```json
{
  "idToken": "token_que_google_devuelve_al_frontend",
  "role": 1
}
```

| Campo | Reglas |
|---|---|
| `idToken` | Requerido · token de Google válido |
| `role` | Solo aplica si el usuario es nuevo — `1` (Candidate) o `2` (Company). Para usuarios existentes se ignora |

**Respuesta exitosa:** igual a login.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"Token de Google inválido: ..."` |
| 400 | `"No se puede registrar un administrador con Google."` |
| 400 | `"Tu cuenta está desactivada. Contacta al soporte."` |

---

### 7. Olvidé mi contraseña
`POST /api/auth/forgot-password` · 🔓 Público · ⚡ 5 req/min por IP

Siempre responde igual aunque el email no exista (por seguridad).

**Body:**
```json
{ "email": "juan@email.com" }
```

| Campo | Reglas |
|---|---|
| `email` | Requerido · formato email válido |

**Respuesta exitosa:**
```json
{ "success": true, "data": null, "message": "Si el email existe, recibirás un enlace para restablecer tu contraseña." }
```

---

### 8. Restablecer contraseña
`POST /api/auth/reset-password` · 🔓 Público · ⚡ 5 req/min por IP

**Body:**
```json
{
  "token": "token_del_link_del_email",
  "newPassword": "NuevoPass.123",
  "confirmPassword": "NuevoPass.123"
}
```

| Campo | Reglas |
|---|---|
| `token` | Requerido |
| `newPassword` | Requerido · mismas reglas de complejidad que `password` en registro |
| `confirmPassword` | Requerido · debe ser idéntico a `newPassword` |

**Respuesta exitosa:**
```json
{ "success": true, "data": null, "message": "Contraseña actualizada correctamente." }
```

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"Las contraseñas no coinciden."` |
| 400 | `"La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial."` |
| 404 | `"El enlace de recuperación es inválido o ya expiró."` |

---

### 9. Cerrar sesión
`POST /api/auth/logout` · 🔐 JWT

**Body:**
```json
{ "refreshToken": "d3f8a2..." }
```

| Campo | Reglas |
|---|---|
| `refreshToken` | Requerido |

**Respuesta exitosa:**
```json
{ "success": true, "data": null, "message": "Sesión cerrada." }
```

---

---

# MÓDULO: CATÁLOGO

Base path: `/api/catalog`

> Cargar al iniciar la app y cachear localmente — los datos no cambian con frecuencia.

---

### 10. Listar categorías
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

### 11. Skills por categoría
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

> **Flujo esperado:** cuando el usuario selecciona una categoría, llamar este endpoint para mostrar sus skills disponibles.

---

---

# MÓDULO: CANDIDATO

Base path: `/api/candidate`

---

### 12. Ver mi perfil
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
      { "skillId": 1, "skillName": "React", "categoryId": 1, "categoryName": "FrontEnd", "level": 4 }
    ]
  },
  "message": null
}
```

---

### 13. Actualizar mi perfil
`PUT /api/candidate/profile` · 👤 Candidate

Todos los campos son opcionales. Al actualizar, el sistema re-evalúa automáticamente al candidato contra todas las ofertas abiertas.

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

| Campo | Reglas |
|---|---|
| `experienceYears` | Opcional · entero ≥ 0 |
| `seniority` | Opcional · `"junior"`, `"mid"` o `"senior"` |
| `englishLevel` | Opcional · `"A1"`, `"A2"`, `"B1"`, `"B2"`, `"C1"` o `"C2"` |
| `githubLink` / `linkedinUrl` / `profilePhotoUrl` | Opcional · string libre |
| `categoryIds` | Opcional · lista de IDs de categoría |
| `skills[].skillId` | Entero positivo |
| `skills[].level` | Entero entre 1 y 5 |

**Respuesta exitosa:** devuelve el perfil completo igual que GET.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"El seniority debe ser junior, mid o senior."` |
| 400 | `"El nivel de inglés debe ser A1, A2, B1, B2, C1 o C2."` |
| 400 | `"El nivel del skill debe estar entre 1 y 5."` |
| 400 | `"Los años de experiencia no pueden ser negativos."` |

---

---

# MÓDULO: EMPRESA

Base path: `/api/company`

---

### 14. Ver mi perfil de empresa
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

### 15. Actualizar perfil de empresa
`PUT /api/company/profile` · 🏢 Company

**Body:**
```json
{ "companyName": "Tech Solutions SAS" }
```

| Campo | Reglas |
|---|---|
| `companyName` | Requerido |

**Respuesta exitosa:** devuelve el perfil completo.

---

---

# MÓDULO: OFERTAS

Base path: `/api/offers`

---

### 16. Ver tiers de precio
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

### 17. Parsear descripción con IA
`POST /api/offers/parse-description` · 🏢 Company

La empresa escribe una descripción libre del cargo y la IA extrae los campos. Útil para pre-llenar el formulario de creación.

**Body:**
```json
{ "rawDescription": "Buscamos un desarrollador React con 2 años de experiencia, inglés B2, trabajo remoto..." }
```

| Campo | Reglas |
|---|---|
| `rawDescription` | Requerido |

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

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"La descripción no puede estar vacía."` |
| 400 | `"Debes completar tu perfil de empresa antes de crear una oferta."` |

---

### 18. Crear oferta
`POST /api/offers` · 🏢 Company

La oferta se crea en estado `PendingPayment`. Para activarla, la empresa debe pagar (ver `/api/payments/create-checkout`).

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

| Campo | Reglas |
|---|---|
| `title` | Requerido |
| `description` | Opcional |
| `salary` | Opcional · número ≥ 0 |
| `modality` | Requerido · `"remote"`, `"onsite"` o `"hybrid"` |
| `minExperienceYears` | Opcional · entero ≥ 0 |
| `requiredEnglishLevel` | Opcional · `"A1"` – `"C2"` |
| `positionsAvailable` | Entero ≥ 1 (default: 1) |
| `tierId` | Requerido · entero positivo |
| `categoryIds` | Opcional · lista de IDs de categoría |
| `skillIds` | Opcional · lista de IDs de skill |

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
    "testSentAt": null,
    "categories": [{ "id": 1, "name": "FrontEnd" }],
    "skills": [{ "id": 4, "name": "React", "categoryId": 1 }],
    "checkoutUrl": null
  },
  "message": "Oferta creada correctamente."
}
```

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"La modalidad debe ser remote, onsite o hybrid."` |
| 400 | `"El nivel de inglés debe ser A1, A2, B1, B2, C1 o C2."` |
| 400 | `"Debe haber al menos 1 posición disponible."` |
| 400 | `"Debes completar tu perfil de empresa antes de crear una oferta."` |
| 400 | `"Debes completar el nombre de la empresa antes de crear una oferta."` |
| 404 | `"Tier de precios no encontrado o inactivo."` |

---

### 19. Ver mis ofertas
`GET /api/offers` · 🏢 Company

**Respuesta exitosa:** `data` es una lista de objetos con la misma forma que en "Crear oferta".

---

### 20. Ver una oferta
`GET /api/offers/{id}` · 🏢 Company

**Respuesta exitosa:** `data` es el objeto de la oferta.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 404 | `"Oferta no encontrada."` |
| 401 | `"No tienes acceso a esta oferta."` |

---

### 21. Editar oferta
`PUT /api/offers/{id}` · 🏢 Company

Solo se pueden editar ofertas en estado `Open`. Todos los campos son opcionales — solo se actualizan los que se envíen.

**Body:**
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

| Campo | Reglas |
|---|---|
| `title` | Opcional · si se envía, no puede ser vacío |
| `salary` | Opcional · número ≥ 0 |
| `modality` | Opcional · `"remote"`, `"onsite"` o `"hybrid"` |
| `minExperienceYears` | Opcional · entero ≥ 0 |
| `requiredEnglishLevel` | Opcional · `"A1"` – `"C2"` |
| `positionsAvailable` | Opcional · entero ≥ 1 |

**Respuesta exitosa:** devuelve la oferta actualizada.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"Solo se pueden editar ofertas en estado Open."` |
| 400 | `"El título no puede estar vacío."` |
| 400 | `"La modalidad debe ser remote, onsite o hybrid."` |
| 400 | `"El nivel de inglés debe ser A1, A2, B1, B2, C1 o C2."` |
| 400 | `"El número de posiciones debe ser al menos 1."` |
| 401 | `"No tienes acceso a esta oferta."` |
| 404 | `"Oferta no encontrada."` |

---

### 22. Cancelar oferta
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
> Si `warning` no es null, muéstrale una confirmación al usuario. Si confirma, llamar `force-cancel`.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"No se puede cancelar una oferta completada."` |
| 400 | `"La oferta ya está cancelada."` |
| 401 | `"No tienes acceso a esta oferta."` |
| 404 | `"Oferta no encontrada."` |

---

### 23. Forzar cancelación
`POST /api/offers/{id}/force-cancel` · 🏢 Company

Cancela aunque haya candidatos en proceso. Llamar solo si el usuario confirmó la advertencia del endpoint anterior.

**Sin body.**

**Respuesta exitosa:**
```json
{ "success": true, "data": null, "message": "Oferta forzada a cancelar correctamente." }
```

---

---

# MÓDULO: PAGOS

Base path: `/api/payments`

---

### 24. Crear link de pago
`POST /api/payments/create-checkout?offerId=7` · 🏢 Company · ⚡ 5 req/5min por usuario

La oferta debe estar en estado `PendingPayment`. Si ya se generó un link antes, devuelve el mismo (idempotente).

**Sin body** — `offerId` va en el query string.

**Respuesta exitosa:**
```json
{ "success": true, "data": { "url": "https://checkout.wompi.co/l/..." }, "message": null }
```
> Redirige al usuario a esa URL. Al pagar, el webhook activa la oferta automáticamente y corre el matching inicial.

---

### 25. Webhook de Wompi
`POST /api/payments/webhook` · 🔓 Público (solo para Wompi)

> **No llamar desde el frontend.** Este endpoint es exclusivo para que Wompi notifique al backend cuando un pago es aprobado. Se configura en el dashboard de Wompi.

---

---

# MÓDULO: MATCHING

Base path: `/api/matching`

---

### 26. Ver ranking de candidatos
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
      "stage": "TestCompleted",
      "aiInsight": "Candidato con sólido dominio de React y experiencia alineada.",
      "aiStrengths": ["React avanzado", "Experiencia en proyectos escalables"],
      "aiOpportunities": ["Puede reforzar TypeScript"],
      "aiRecommendation": "Recomendado para el test técnico.",
      "matchedSkills": ["React", "JavaScript"],
      "createdAt": "2026-06-25T15:00:00Z",
      "testScore": 82.50,
      "testFeedback": "Buen desempeño general. El código del challenge es funcional aunque podría optimizarse."
    }
  ],
  "message": null
}
```
> Lista ordenada por `adjustedScore` desc (o `matchPercentage` si no hay score de IA).
> `aiInsight`, `aiStrengths`, `aiOpportunities`, `aiRecommendation` pueden ser `null` — solo el top 3 recibe evaluación automática de fit antes del test.
> `testScore` y `testFeedback` son `null` hasta que el candidato complete y envíe el test (`stage = "TestCompleted"` o `"Selected"`). Son el score 0–100 y el feedback resumen que generó la IA al evaluar las respuestas.
> El email real del candidato solo está visible cuando `stage = "Selected"`.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 401 | `"No tienes acceso a esta oferta."` |
| 404 | `"Oferta no encontrada."` |

---

### 27. Ejecutar matching manualmente
`POST /api/matching/{offerId}/run` · 🏢 Company

Corre el matching incremental (solo candidatos nuevos sin match previo). Normalmente ocurre automático al activar la oferta; este endpoint permite forzarlo.

**Sin body.**

**Respuesta exitosa:** lista de matches igual que en "Ver ranking".

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"El matching solo se puede ejecutar sobre ofertas en estado Open."` |
| 401 | `"No tienes acceso a esta oferta."` |
| 404 | `"Oferta no encontrada."` |

---

### 28. Reevaluar ranking completo
`POST /api/matching/{offerId}/reevaluate` · 🏢 Company

Recalcula el score de TODOS los candidatos. Útil cuando la oferta fue editada.

**Sin body.**

**Respuesta exitosa:**
```json
{ "success": true, "data": [ /* lista completa actualizada */ ], "message": "Reevaluación completada." }
```

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"Solo se puede reevaluar una oferta en estado Open."` |
| 401 | `"No tienes acceso a esta oferta."` |
| 404 | `"Oferta no encontrada."` |

---

### 29. Enviar test a candidatos
`POST /api/matching/send-test` · 🏢 Company

Los candidatos reciben un email con link directo al test. Solo se puede enviar a candidatos en stage `Matched`.

**Body:**
```json
{ "matchIds": [12, 15, 18] }
```

| Campo | Reglas |
|---|---|
| `matchIds` | Requerido · lista de IDs de match (no de candidatos) |

**Respuesta exitosa:**
```json
{ "success": true, "data": null, "message": "Tests enviados correctamente. Los candidatos recibirán un correo con el enlace." }
```

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"Debes seleccionar al menos un candidato."` |
| 400 | `"Todos los matches deben pertenecer a la misma oferta."` |
| 400 | `"Solo se puede enviar el test a candidatos en estado Matched. Los siguientes ya tienen otro estado: 12, 15"` |
| 400 | `"La oferta aún no tiene un test generado. Espera a que la IA genere el test."` |
| 400 | `"El tier de esta oferta permite máximo X candidatos con test. Ya tienes Y y estás intentando agregar Z más."` |
| 401 | `"No tienes acceso a esta oferta."` |
| 404 | `"Uno o más matches no fueron encontrados."` |

---

### 30. Seleccionar candidato
`POST /api/matching/{matchId}/select` · 🏢 Company

Solo desde stage `TestCompleted`. Si al seleccionar se llenan todas las posiciones disponibles, la oferta pasa a `Completed` automáticamente. El candidato recibe un email de notificación (best-effort — si el envío falla, la selección igual se guarda).

**Sin body.**

**Respuesta exitosa:**
```json
{ "success": true, "data": { /* objeto del match actualizado */ }, "message": "Candidato seleccionado correctamente." }
```

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"Solo puedes seleccionar candidatos que hayan completado el test."` |
| 401 | `"No tienes acceso a este match."` |
| 404 | `"Match no encontrado."` |

---

### 31. Rechazar candidato
`POST /api/matching/{matchId}/reject` · 🏢 Company

Se puede rechazar desde `Matched`, `TestSent` o `TestCompleted`. No desde `Selected`. El candidato recibe un email empático notificando que el proceso no continuó (best-effort).

**Sin body.**

**Respuesta exitosa:**
```json
{ "success": true, "data": null, "message": "Candidato rechazado correctamente." }
```

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"No se puede rechazar un candidato que ya fue seleccionado."` |
| 400 | `"El candidato ya fue rechazado."` |
| 401 | `"No tienes acceso a este match."` |
| 404 | `"Match no encontrado."` |

---

---

# MÓDULO: TESTS

Base path: `/api/tests`

---

### 32. Generar test con IA
`POST /api/tests/{offerId}/generate` · 🏢 Company

Genera el test técnico para la oferta. Se hace una sola vez; para reemplazarlo usar `regenerate`.

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
        "options": { "A": "No hay diferencia", "B": "=== compara tipo además del valor", "C": "== es más estricto", "D": "Solo se usa == en JavaScript moderno" },
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

### 33. Regenerar test
`POST /api/tests/{offerId}/regenerate` · 🏢 Company

Reemplaza el test existente con uno nuevo generado por IA. **Borra el test anterior.**

**Sin body.** Respuesta igual que generar.

---

### 34. Ver test completo (empresa)
`GET /api/tests/{offerId}` · 🏢 Company

Devuelve el test con `correctAnswer` y `explanation` visibles.

**Respuesta:** igual que generar.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 401 | `"No tienes acceso a esta oferta."` |
| 404 | `"Esta oferta aún no tiene un test generado."` |

---

### 35. Ver historial de chat de una pregunta
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

### 36. Editar pregunta con IA (chat)
`POST /api/tests/questions/{questionId}/chat` · 🏢 Company

**Body:**
```json
{ "message": "Hazla más difícil y enfócala en hooks de React" }
```

| Campo | Reglas |
|---|---|
| `message` | Requerido |

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

### 37. Listar mis tests (candidato)
`GET /api/tests/candidate` · 👤 Candidate

Devuelve todos los tests a los que el candidato ha sido invitado, ordenados por deadline descendente.

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": [
    {
      "testId": 3,
      "offerId": 7,
      "offerTitle": "Desarrollador React Senior",
      "testTitle": "Test técnico — React Senior",
      "status": "Pending",
      "startedAt": null,
      "deadline": "2026-06-29T15:00:00Z",
      "timeLimitMinutes": 45,
      "score": null
    },
    {
      "testId": 5,
      "offerId": 9,
      "offerTitle": "Backend Node.js",
      "testTitle": "Test técnico — Node.js",
      "status": "Evaluated",
      "startedAt": "2026-06-25T10:00:00Z",
      "deadline": "2026-06-27T10:00:00Z",
      "timeLimitMinutes": 30,
      "score": 82.50
    }
  ],
  "message": null
}
```

> **Lógica de estado en el frontend:**
> - `status = "Pending"` + `startedAt = null` → **Sin realizar** (mostrar botón "Ir al test")
> - `status = "Pending"` + `startedAt != null` → **En curso** (mostrar botón "Continuar")
> - `status = "Evaluated"` → **Completado** (mostrar score y botón "Ver resultado")
> - `status = "Expired"` → **Expirado** (mostrar mensaje, sin acción)
>
> Usar `offerId` para navegar a `preview` → `start`. Usar `testId` para navegar a `result`.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 404 | `"Perfil de candidato no encontrado."` |

---

### 38. Ver resumen del test sin iniciarlo (candidato)
`GET /api/tests/{offerId}/candidate/preview` · 👤 Candidate

Devuelve solo los metadatos del test — título, tiempo límite y conteo de preguntas por tipo. **No toca `startedAt` ni inicia el cronómetro.** Usar para mostrarle al candidato qué le espera antes de que confirme que quiere empezar.

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": {
    "testId": 3,
    "title": "Test técnico — Desarrollador React Senior",
    "timeLimitMinutes": 45,
    "totalQuestions": 6,
    "multipleChoiceCount": 5,
    "codeChallengeCount": 1
  },
  "message": null
}
```

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"El plazo para rendir este test ha expirado."` |
| 400 | `"Ya enviaste tus respuestas. Espera los resultados."` |
| 401 | `"No estás invitado a rendir este test."` |
| 404 | `"Test no encontrado."` |
| 404 | `"Perfil de candidato no encontrado."` |

---

### 38. Iniciar test y obtener preguntas (candidato)
`POST /api/tests/{offerId}/candidate/start` · 👤 Candidate

**Sin body.** Registra `startedAt` en el primer acceso e inicia el cronómetro. Devuelve las preguntas **sin** respuestas correctas ni hints. Llamar únicamente cuando el candidato confirme "quiero empezar" — no hay vuelta atrás.

**Respuesta exitosa:** igual que generar test, pero `correctAnswer`, `explanation`, `isGorilla` y `gorillaHint` siempre vienen `null`.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"El plazo para rendir este test ha expirado."` |
| 400 | `"Ya enviaste tus respuestas. Espera los resultados."` |
| 401 | `"No estás invitado a rendir este test."` |
| 404 | `"Test no encontrado."` |
| 404 | `"Perfil de candidato no encontrado."` |

---

### 39. Enviar respuestas
`POST /api/tests/{testId}/submit` · 👤 Candidate

**Body:**
```json
{
  "answers": [
    { "questionId": 10, "selectedOption": null, "codeSubmitted": "function solve(arr) { return Math.max(...arr); }" },
    { "questionId": 11, "selectedOption": "B",  "codeSubmitted": null }
  ]
}
```

| Campo | Reglas |
|---|---|
| `answers` | Requerido |
| `answers[].questionId` | Entero positivo |
| `answers[].selectedOption` | Opcional · solo `"A"`, `"B"`, `"C"` o `"D"` (para MultipleChoice) |
| `answers[].codeSubmitted` | Opcional · código libre (para CodeChallenge) |

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
      { "questionId": 10, "isCorrect": true,  "feedback": "Solución correcta y eficiente." },
      { "questionId": 11, "isCorrect": true,  "feedback": null }
    ]
  },
  "message": "Respuestas enviadas y evaluadas correctamente."
}
```

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"Debes incluir al menos una respuesta."` |
| 400 | `"La opción seleccionada debe ser A, B, C o D."` |
| 400 | `"El plazo para rendir este test ha expirado."` |
| 400 | `"Tiempo agotado. El test debía completarse en X minutos."` |
| 400 | `"Ya enviaste tus respuestas."` |
| 401 | `"No tienes una submission activa para este test."` |
| 404 | `"Perfil de candidato no encontrado."` |

---

### 40. Ver resultado de un test
`GET /api/tests/{testId}/result` · 👤 Candidate

Devuelve el resultado si ya fue evaluado.

**Respuesta:** igual que enviar respuestas.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"Aún no has enviado tus respuestas."` |
| 401 | `"No tienes una submission para este test."` |
| 404 | `"Perfil de candidato no encontrado."` |

---

---

# MÓDULO: ADMIN

Base path: `/api/admin`

---

### 40. Listar usuarios
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

### 41. Ver usuario por ID
`GET /api/admin/users/{userId}` · 🛡️ Admin

**Respuesta exitosa:** objeto igual al de la lista.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 404 | `"Usuario {id} no encontrado."` |

---

### 42. Crear administrador
`POST /api/admin/users` · 🛡️ Admin

Único endpoint para crear cuentas de tipo Admin. El registro público bloquea este rol.

**Body:**
```json
{
  "fullName": "Admin MatchIQ",
  "email": "admin@matchiq.co",
  "cedula": "9876543210",
  "password": "AdminPass.1",
  "confirmPassword": "AdminPass.1"
}
```

| Campo | Reglas |
|---|---|
| `fullName` | Requerido |
| `email` | Requerido · formato email válido |
| `cedula` | Requerido |
| `password` | Requerido · mismas reglas de complejidad que el registro |
| `confirmPassword` | Requerido · debe ser idéntico a `password` |

**Respuesta exitosa:**
```json
{ "success": true, "data": null, "message": "Administrador creado correctamente." }
```

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"Las contraseñas no coinciden."` |
| 400 | `"La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial."` |
| 400 | `"El email ya está registrado."` |
| 400 | `"La cédula ya está registrada."` |

---

### 43. Activar / desactivar cuenta
`PATCH /api/admin/users/{userId}/toggle-status` · 🛡️ Admin

**Sin body.**

**Respuesta exitosa:**
```json
{ "success": true, "data": { /* objeto del usuario con el nuevo estado */ }, "message": "Cuenta desactivada correctamente." }
```

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"No se puede desactivar la cuenta de un administrador."` |
| 404 | `"Usuario {id} no encontrado."` |

---

### 44. Eliminar usuario
`DELETE /api/admin/users/{userId}` · 🛡️ Admin

Elimina en cascada: perfil, ofertas, matches, submissions, etc.

**Sin body.**

**Respuesta exitosa:**
```json
{ "success": true, "data": null, "message": "Usuario eliminado correctamente." }
```

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"No se puede eliminar la cuenta de un administrador."` |
| 404 | `"Usuario {id} no encontrado."` |

---

### 45. Estadísticas del sistema
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

## Flujos por rol

### Flujo Empresa (Company)
```
1.  register → 2. verify-email → 3. login
4.  company/profile (PUT) — completar nombre de empresa
5.  catalog/categories + catalog/categories/{id}/skills — cargar catálogo
6.  offers/tiers — mostrar opciones de precio
7.  offers/parse-description (opcional) — pre-llenar formulario con IA
8.  offers (POST) — crear oferta → queda en PendingPayment
9.  payments/create-checkout?offerId=X → redirigir al usuario a Wompi
    [Wompi confirma el pago → webhook activa la oferta → corre matching inicial]
10. tests/{offerId}/generate — generar test técnico
11. tests/questions/{id}/chat (POST) — ajustar preguntas con IA (opcional)
12. matching/{offerId} (GET) — ver ranking de candidatos
13. matching/send-test (POST) — enviar test a candidatos seleccionados
14. matching/{offerId} (GET) — ver ranking con `testScore` y `testFeedback` de quienes completaron el test
15. matching/{matchId}/select o /reject — decisión final basada en score del test
```

### Flujo Candidato (Candidate)
```
1. register → 2. verify-email → 3. login
4. candidate/profile (PUT) — completar perfil (dispara matching automático)
   [El sistema evalúa al candidato contra todas las ofertas abiertas]
   [Candidato recibe email de invitación cuando la empresa le envía el test]
5. tests/candidate (GET) — sección "Mis tests": lista todos sus tests con estado
   [Sin realizar / En curso / Completado / Expirado]
6. tests/{offerId}/candidate/preview (GET) — ver resumen del test (sin iniciar cronómetro)
   [Mostrar: título, tiempo límite, número y tipos de preguntas]
   [El candidato confirma "Quiero empezar"]
7. tests/{offerId}/candidate/start (POST) — iniciar test formalmente (inicia cronómetro)
8. tests/{testId}/submit (POST) — enviar respuestas
9. tests/{testId}/result (GET) — ver calificación y feedback
```

---

## Notas de implementación

1. **Manejo del token:** al recibir `401`, intentar renovar con `/api/auth/refresh`. Si el refresh también falla, redirigir al login.
2. **Tiempo del test:** guardar `timeLimitMinutes` desde la respuesta de `preview`. Al hacer `start`, comenzar el contador en el cliente. Si el tiempo corre, el backend rechaza el submit con `400` y marca la submission como expirada.
3. **Flujo de test en dos pasos:** llamar `preview` para mostrar qué le espera al candidato. Solo llamar `start` cuando el candidato confirme explícitamente — ese POST registra `startedAt` y no hay vuelta atrás.
4. **Estado de la oferta:** cuando `status = "PendingPayment"`, mostrar el botón de pago. `checkoutUrl` puede ser `null` si aún no se generó el link — llamar `create-checkout` para obtenerlo.
5. **Email del candidato en matching:** el email real solo aparece cuando `stage = "Selected"`. En etapas anteriores puede venir enmascarado.
6. **Score del test en matching:** `testScore` y `testFeedback` son `null` mientras el candidato no haya enviado sus respuestas. Aparecen automáticamente en `GET /api/matching/{offerId}` en cuanto el candidato completa el test — no hay que hacer ninguna llamada extra. Usarlos para la pantalla de decisión (select/reject).
7. **Rate limits:** al recibir `429`, esperar al menos 60 segundos antes de reintentar. No mostrar un spinner — informar al usuario.
8. **Catálogo:** cargar categorías y skills una sola vez al iniciar la app y cachear — no cambian con frecuencia.
