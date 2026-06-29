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
| `409` | Conflicto de estado — requiere confirmación del usuario antes de continuar |
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

## Regla clave: ventana de edición de oferta

> **Todo puede editarse mientras la oferta esté en `PendingPayment`** (antes de pagar): detalles de la oferta, generar/regenerar test completo, editar preguntas individualmente.
>
> **Una vez pagada (`Open` en adelante), nada puede modificarse.** El pago es el punto de no retorno. Para hacer una nueva reclutación con cambios, la empresa debe crear una nueva oferta.

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

La oferta se crea en estado `PendingPayment`. En este estado la empresa puede editar libremente la oferta, generar el test y ajustar las preguntas. Al pagar, todo queda bloqueado.

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
  "testDeadlineDays": 3,
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
| `testDeadlineDays` | Requerido · entero entre 1 y 90 · días que tendrá el candidato para abrir y comenzar el test desde que se le envía |
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
    "testDeadlineDays": 3,
    "status": "PendingPayment",
    "createdAt": "2026-06-25T14:00:00Z",
    "paidAt": null,
    "expiresAt": null,
    "categories": [{ "id": 1, "name": "FrontEnd" }],
    "skills": [{ "id": 4, "name": "React", "categoryId": 1 }]
  },
  "message": "Oferta creada correctamente."
}
```

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"La modalidad debe ser remote, onsite o hybrid."` |
| 400 | `"El nivel de inglés debe ser A1, A2, B1, B2, C1 o C2."` |
| 400 | `"El plazo para el test debe ser entre 1 y 90 días."` |
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

**Solo disponible mientras la oferta esté en `PendingPayment`.** Una vez pagada, la oferta es inmutable. Todos los campos son opcionales — solo se actualizan los que se envíen.

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
| 400 | `"La oferta no puede ser modificada una vez que ha sido activada."` |
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

**Respuesta exitosa (cancelación directa):**
```json
{
  "success": true,
  "data": { "cancelled": true, "warning": null, "candidatesInProgress": 0 },
  "message": "Oferta cancelada correctamente."
}
```

**Respuesta 409 (hay candidatos en proceso — requiere confirmación):**
```json
{
  "success": false,
  "data": { "cancelled": false, "warning": "Hay 2 candidato(s) con un test en proceso. ¿Confirmas la cancelación?", "candidatesInProgress": 2 },
  "message": "Hay 2 candidato(s) con un test en proceso. ¿Confirmas la cancelación?"
}
```
> Si recibes `409`, mostrar un diálogo de confirmación con el `warning`. Si el usuario confirma, llamar a `force-cancel`.

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

> **Pasarela activa: Stripe Checkout** — Stripe hospeda la página de pago con tarjeta. Para desarrollo se usan las tarjetas de prueba de Stripe (ver sección al final).

---

### 24. Crear sesión de pago (Stripe)
`POST /api/payments/create-checkout?offerId=7` · 🏢 Company · ⚡ 5 req/5min por usuario

La oferta debe estar en estado `PendingPayment`. Si ya existe una sesión abierta para esta oferta, devuelve la misma URL (idempotente). Si la sesión ya fue pagada pero `verify-session` nunca fue llamado, el backend activa la oferta automáticamente y responde con un 400.

**Sin body** — `offerId` va en el query string.

**Respuesta exitosa:**
```json
{ "success": true, "data": { "url": "https://checkout.stripe.com/pay/cs_test_..." }, "message": null }
```

> **Flujo frontend:**
> 1. Llamar este endpoint y redirigir al usuario a `data.url` (Stripe Checkout hospedado).
> 2. Stripe redirige de vuelta a `SuccessUrl?offerId={id}&session_id={CHECKOUT_SESSION_ID}` al pagar.
> 3. **Inmediatamente al aterrizaje en la pantalla de éxito**, llamar `POST /api/payments/verify-session?sessionId={session_id}` para activar la oferta.
> 4. Si el usuario cierra la ventana antes de confirmar el pago, Stripe redirige a `CancelUrl?offerId={id}&success=false`. No se cobra nada; la sesión de Stripe queda abierta y al volver a llamar `create-checkout` se recupera la misma URL.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"La oferta no está pendiente de pago."` |
| 400 | `"El pago ya fue procesado. La oferta ha sido activada."` |
| 404 | `"Perfil de empresa no encontrado."` |
| 404 | `"Oferta no encontrada."` |

---

### 25. Verificar y activar pago
`POST /api/payments/verify-session?sessionId=cs_test_...` · 🏢 Company

Consulta directamente la API de Stripe con el `session_id` recibido en la redirección de éxito. Si Stripe confirma `payment_status = "paid"`, activa la oferta (`PendingPayment` → `Open`), ejecuta el matching inicial y bloquea ediciones futuras. Idempotente — si ya fue activada, devuelve `activated: true` sin efectos secundarios.

**Sin body** — `sessionId` va en el query string.

**Respuesta exitosa (pago aprobado y oferta activada):**
```json
{ "success": true, "data": { "activated": true }, "message": "Pago verificado. Oferta activada." }
```

**Respuesta exitosa (Stripe aún no confirmó el pago):**
```json
{ "success": true, "data": { "activated": false }, "message": "El pago aún no ha sido procesado." }
```

> Si `activated = false`, el pago no completó todavía. Mostrar feedback al usuario y permitirle reintentar en unos segundos.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"No se pudo verificar la sesión de pago: ..."` |
| 401 | `"No tienes acceso a esta oferta."` |
| 404 | `"No se encontró el registro de pago para esta sesión."` |
| 404 | `"Perfil de empresa no encontrado."` |

---

### 26. Webhook de Stripe
`POST /api/payments/webhook` · 🔓 Público (solo para Stripe)

> **No llamar desde el frontend.** Endpoint para que Stripe notifique eventos de pago al backend. En el MVP local no se configura (el flujo usa `verify-session` en su lugar). En producción se configura en el dashboard de Stripe con el evento `checkout.session.completed`. Cuando `Stripe:WebhookSecret` está vacío en la configuración, el backend procesa el evento sin verificar la firma (solo desarrollo).

---

---

# MÓDULO: MATCHING

Base path: `/api/matching`

---

### 27. Ver ranking de candidatos
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
      "email": null,
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
> `aiInsight`, `aiStrengths`, `aiOpportunities`, `aiRecommendation` pueden ser `null` — solo el top 3 recibe evaluación automática de fit.
> `testScore` y `testFeedback` son `null` hasta que el candidato complete el test (`stage = "TestCompleted"` o `"Selected"`).
> `email` es `null` en todos los stages excepto `"Selected"`.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 401 | `"No tienes acceso a esta oferta."` |
| 404 | `"Oferta no encontrada."` |

---

### 28. Ejecutar matching manualmente
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

### 29. Reevaluar ranking completo
`POST /api/matching/{offerId}/reevaluate` · 🏢 Company

Recalcula el score de TODOS los candidatos.

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

### 30. Enviar test a candidatos
`POST /api/matching/send-test` · 🏢 Company

Los candidatos reciben un email con link directo al test. Solo se puede enviar a candidatos en stage `Matched`. El candidato tendrá `testDeadlineDays` días (definido al crear la oferta) para abrir el test.

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

### 31. Seleccionar candidato
`POST /api/matching/{matchId}/select` · 🏢 Company

Solo desde stage `TestCompleted`. Si al seleccionar se llenan todas las posiciones disponibles, la oferta pasa a `Completed` automáticamente. El candidato recibe un email de notificación (best-effort).

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

### 32. Rechazar candidato
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

### 33. Generar test con IA
`POST /api/tests/{offerId}/generate` · 🏢 Company

Genera el test técnico para la oferta usando IA. Disponible en `PendingPayment` (para preview antes de pagar) y en `Open` si aún no existe test. Si ya existe un test, retorna el existente sin llamar a la IA.

**Body:**
```json
{ "timeLimitMinutes": 45 }
```

| Campo | Reglas |
|---|---|
| `timeLimitMinutes` | Requerido · entero ≥ 1 · minutos que tendrá el candidato para completar el test una vez que lo inicie |

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

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"El tiempo límite debe ser al menos 1 minuto."` |
| 401 | `"No tienes acceso a esta oferta."` |
| 404 | `"Oferta no encontrada."` |

---

### 34. Regenerar test completo
`POST /api/tests/{offerId}/regenerate` · 🏢 Company

Reemplaza el test existente con uno completamente nuevo generado por IA. **Borra el test anterior y todo su historial de chat.** Solo disponible en `PendingPayment` — una vez pagada la oferta, no se puede regenerar.

**Body:**
```json
{ "timeLimitMinutes": 60 }
```

| Campo | Reglas |
|---|---|
| `timeLimitMinutes` | Requerido · entero ≥ 1 |

**Respuesta:** igual que generar test.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"No se puede regenerar el test después de haber activado la oferta."` |
| 400 | `"El tiempo límite debe ser al menos 1 minuto."` |
| 401 | `"No tienes acceso a esta oferta."` |
| 404 | `"Oferta no encontrada."` |

---

### 35. Ver test completo (empresa)
`GET /api/tests/{offerId}` · 🏢 Company

Devuelve el test con `correctAnswer` y `explanation` visibles.

**Respuesta:** igual que generar.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 401 | `"No tienes acceso a esta oferta."` |
| 404 | `"Esta oferta aún no tiene un test generado."` |

---

### 36. Ver historial de chat de una pregunta
`GET /api/tests/questions/{questionId}/chat` · 🏢 Company

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": [
    { "role": "admin", "content": "Cambia el nivel a más difícil", "createdAt": "2026-06-25T15:00:00Z" },
    { "role": "assistant", "content": "He actualizado la pregunta según tu solicitud.", "createdAt": "2026-06-25T15:00:05Z" }
  ],
  "message": null
}
```

---

### 37. Editar pregunta con IA (chat)
`POST /api/tests/questions/{questionId}/chat` · 🏢 Company

Permite modificar una pregunta específica mediante instrucciones en lenguaje natural. **Solo disponible en `PendingPayment`** — una vez pagada la oferta, las preguntas son inmutables.

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
    "assistantMessage": "He actualizado la pregunta según tu solicitud."
  },
  "message": null
}
```

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"El mensaje no puede estar vacío."` |
| 400 | `"No se pueden modificar preguntas una vez que la oferta ha sido activada."` |
| 401 | `"No tienes acceso a esta pregunta."` |
| 404 | `"Pregunta no encontrada."` |

---

### 38. Listar mis tests (candidato)
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
> - `status = "Pending"` + `startedAt != null` → **En curso o evaluación pendiente** (mostrar botón "Continuar" o mensaje "Resultado próximamente")
> - `status = "Evaluated"` → **Completado** (mostrar score y botón "Ver resultado")
> - `status = "Expired"` → **Expirado** (mostrar mensaje, sin acción)
>
> Usar `offerId` para navegar a `preview` → `start`. Usar `testId` para navegar a `result`.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 404 | `"Perfil de candidato no encontrado."` |

---

### 39. Ver resumen del test sin iniciarlo (candidato)
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

### 40. Iniciar test y obtener preguntas (candidato)
`POST /api/tests/{offerId}/candidate/start` · 👤 Candidate

**Sin body.** Registra `startedAt` en el primer acceso y recalcula el deadline al `startedAt + timeLimitMinutes`. Devuelve las preguntas **sin** respuestas correctas ni hints. Llamar únicamente cuando el candidato confirme "quiero empezar" — no hay vuelta atrás.

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": {
    "submissionId": 456,
    "test": {
      "id": 3,
      "offerId": 7,
      "title": "Test técnico — Desarrollador React Senior",
      "timeLimitMinutes": 45,
      "createdAt": "2026-06-25T14:30:00Z",
      "questions": [ /* igual que generar test, pero correctAnswer, explanation, isGorilla y gorillaHint vienen null */ ]
    }
  },
  "message": null
}
```

> **Importante:** la respuesta tiene dos campos raíz — `submissionId` y `test`. El test con las preguntas vive dentro de `test`. Guardar `submissionId` de inmediato y pasarlo al servicio de proctoring (Python) al iniciar la sesión de monitoreo.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"El plazo para rendir este test ha expirado."` |
| 400 | `"Ya enviaste tus respuestas. Espera los resultados."` |
| 401 | `"No estás invitado a rendir este test."` |
| 404 | `"Test no encontrado."` |
| 404 | `"Perfil de candidato no encontrado."` |

---

### 41. Enviar respuestas
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
| `answers` | Requerido · al menos 1 respuesta |
| `answers[].questionId` | Entero positivo |
| `answers[].selectedOption` | Opcional · solo `"A"`, `"B"`, `"C"` o `"D"` (para MultipleChoice) |
| `answers[].codeSubmitted` | Opcional · código libre (para CodeChallenge) |

**Respuesta exitosa — evaluación completada:**
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

**Respuesta exitosa — evaluación en cola (IA falló temporalmente):**
```json
{
  "success": true,
  "data": {
    "score": null,
    "feedback": null,
    "status": "Pending",
    "submittedAt": "2026-06-25T16:00:00Z",
    "aiEvaluatedAt": null,
    "questionResults": []
  },
  "message": "Respuestas enviadas y evaluadas correctamente."
}
```
> Si `status = "Pending"`, las respuestas fueron guardadas pero la evaluación de IA está en proceso. El backend reintenta automáticamente cada 24h. El candidato puede consultar `GET /api/tests/{testId}/result` más tarde para ver el score.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"Debes incluir al menos una respuesta."` |
| 400 | `"Tus respuestas ya fueron recibidas. El resultado estará disponible pronto."` |
| 400 | `"El plazo para rendir este test ha expirado."` |
| 400 | `"Ya enviaste tus respuestas."` |
| 401 | `"No tienes una submission activa para este test."` |
| 404 | `"Perfil de candidato no encontrado."` |

---

### 42. Ver resultado de un test
`GET /api/tests/{testId}/result` · 👤 Candidate

Devuelve el resultado si ya fue evaluado.

**Respuesta:** igual que enviar respuestas.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 400 | `"Tus respuestas fueron recibidas. El resultado estará disponible pronto."` |
| 400 | `"Aún no has enviado tus respuestas."` |
| 401 | `"No tienes una submission para este test."` |
| 404 | `"Perfil de candidato no encontrado."` |

---

### 43. Ver reporte de proctoring de un candidato (empresa)
`GET /api/tests/submissions/{matchId}/proctoring` · 🏢 Company

Devuelve el reporte comportamental de la sesión de proctoring (monitoreo de IA) del candidato durante el test. Solo disponible después de que el candidato completó el test (`stage >= TestCompleted`). El score de integridad y el resumen de IA se calculan la primera vez que se consulta y se cachean en BD.

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": {
    "sessionId": "550e8400-e29b-41d4-a716-446655440000",
    "inicio": "2026-06-28T15:00:00Z",
    "fin": "2026-06-28T16:00:00Z",
    "totalFramesProcesados": 7200,
    "totalEventos": 2,
    "integrityScore": 60.0,
    "integritySummary": "Durante la sesión se detectaron dos incidentes: uso de dispositivo adicional y presencia de una segunda persona. El score de integridad de 60/100 indica riesgo moderado.",
    "eventos": [
      {
        "tipo": "dispositivo_prohibido",
        "detalle": "Detectado: cell phone",
        "evidencia": "ruta/imagen.jpg",
        "timestamp": "2026-06-28T15:10:00Z"
      },
      {
        "tipo": "segunda_persona",
        "detalle": "Intruso presente por más de 10s",
        "evidencia": null,
        "timestamp": "2026-06-28T15:40:00Z"
      }
    ]
  }
}
```

**Fórmula del `integrityScore`:** `max(0, 100 - Σpenalties)`. Penalizaciones por tipo: `camara_cubierta` -30, `dispositivo_prohibido` -20, `segunda_persona` -20, `rostro_ausente` -15, `distraccion` -8, otros -5.

**Valores de `tipo` en eventos:**
- `"dispositivo_prohibido"` — se detectó un segundo dispositivo (celular, tablet, etc.)
- `"distraccion"` — el candidato estuvo fuera de pantalla por más de 10 segundos
- `"segunda_persona"` — se detectó otra persona en el frame
- `"camara_cubierta"` — la cámara fue tapada o bloqueada
- `"rostro_ausente"` — no se detectó ningún rostro en el frame por tiempo prolongado

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 401 | Sin token o sin rol Company |
| 403 | El match no pertenece a una oferta de esta empresa |
| 400 | El candidato todavía no completó el test |
| 404 | Match, submission o sesión de proctoring no encontrada |

---

---

# MÓDULO: ADMIN

Base path: `/api/admin`

---

### 43. Listar usuarios
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

### 44. Ver usuario por ID
`GET /api/admin/users/{userId}` · 🛡️ Admin

**Respuesta exitosa:** objeto igual al de la lista.

**Errores posibles:**

| HTTP | `message` |
|---|---|
| 404 | `"Usuario {id} no encontrado."` |

---

### 45. Crear administrador
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

### 46. Activar / desactivar cuenta
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

### 47. Eliminar usuario
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

### 48. Estadísticas del sistema
`GET /api/admin/stats` · 🛡️ Admin

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": {
    "totalCandidates": 120,
    "totalCompanies": 35,
    "usersRegisteredLast30Days": 45,

    "totalOffers": 58,
    "offersCreatedLast30Days": 10,
    "offersActive": 18,
    "offersCompleted": 20,
    "offersCancelled": 3,
    "offersExpired": 1,
    "offersPendingPayment": 5,
    "offersByStatus": {
      "PendingPayment": 5,
      "Open": 18,
      "Completed": 20,
      "Cancelled": 3,
      "Expired": 1
    },

    "totalMatches": 430,
    "matchesSelected": 28,
    "matchesRejected": 45,
    "matchesTestSent": 62,
    "matchesTestCompleted": 38,

    "activeTests": 12,
    "pendingSubmissions": 8,
    "submissionsEvaluated": 95,
    "submissionsExpired": 12,
    "averageTestScore": 74.3,

    "totalRevenueCop": 4850000.00,
    "paymentsCompleted": 24,
    "paymentsPending": 3,

    "testCompletionRate": 88.8,
    "selectionRate": 25.2
  },
  "message": null
}
```

> **Descripción de los campos de tasas:**
> - `testCompletionRate`: porcentaje de submissions evaluadas vs. (evaluadas + expiradas). Mide cuántos candidatos completaron el test a tiempo.
> - `selectionRate`: porcentaje de candidatos seleccionados vs. (testCompleted + selected + rejected). Mide qué tan exigente está siendo el proceso de selección.

---

---

## Flujos por rol

### Flujo Empresa (Company)
```
1.  register → 2. verify-email → 3. login
4.  company/profile (PUT) — completar nombre de empresa
5.  catalog/categories + catalog/categories/{id}/skills — cargar catálogo
6.  offers/tiers — mostrar opciones de precio al usuario
7.  offers/parse-description (opcional) — pre-llenar formulario con IA

── VENTANA DE EDICIÓN LIBRE (todo en PendingPayment) ──────────────────────────
8.  offers (POST) — crear oferta con testDeadlineDays → queda en PendingPayment
9.  tests/{offerId}/generate (POST con { timeLimitMinutes }) — generar test con IA
10. tests/questions/{id}/chat (POST) — ajustar preguntas individualmente (opcional)
    tests/{offerId}/regenerate (POST con { timeLimitMinutes }) — regenerar test completo (opcional)
    offers/{id} (PUT) — editar detalles de la oferta si es necesario (opcional)
    → Repetir 9-10 hasta quedar conforme

── PAGO = PUNTO DE NO RETORNO ─────────────────────────────────────────────────
11. payments/create-checkout?offerId=X → redirigir al usuario a Stripe Checkout
    [Usuario paga → Stripe redirige a SuccessUrl con session_id en la URL]
    [Frontend llama payments/verify-session?sessionId=... → activa oferta (Open) → matching inicial corre automático]
    [A partir de aquí: oferta, test y preguntas son inmutables]

── PROCESO DE SELECCIÓN ────────────────────────────────────────────────────────
12. matching/{offerId} (GET) — ver ranking de candidatos matcheados
13. matching/send-test (POST) — enviar test a candidatos seleccionados del ranking
14. matching/{offerId} (GET) — ver ranking con testScore y testFeedback de quienes completaron
15. matching/{matchId}/select o /reject — decisión final basada en el test
```

### Flujo Candidato (Candidate)
```
1. register → 2. verify-email → 3. login
4. candidate/profile (PUT) — completar perfil (dispara matching automático)
   [El sistema evalúa al candidato contra todas las ofertas abiertas]
   [Candidato recibe email de invitación cuando la empresa le envía el test]
5. tests/candidate (GET) — sección "Mis tests": lista todos sus tests con estado
6. tests/{offerId}/candidate/preview (GET) — ver resumen antes de empezar (sin iniciar cronómetro)
7. tests/{offerId}/candidate/start (POST) — confirmar inicio (inicia cronómetro real en backend)
8. tests/{testId}/submit (POST) — enviar respuestas
9. tests/{testId}/result (GET) — ver calificación y feedback
   [Candidato recibe email si es seleccionado o rechazado]
```

---

## Tarjetas de prueba — Stripe

```
Pago exitoso             4242 4242 4242 4242
Pago declinado           4000 0000 0000 0002
Fondos insuficientes     4000 0000 0000 9995
Tarjeta expirada         4000 0000 0000 0069
CVC incorrecto           4000 0000 0000 0127
Requiere autenticación   4000 0025 0000 3155  (3D Secure)

En todos usar:
  Fecha: cualquiera futura (ej. 12/29)
  CVC: cualquier 3 dígitos
  Nombre: cualquier texto
```

> Las tarjetas de prueba solo funcionan en modo test de Stripe (`pk_test_...`). En producción se usan tarjetas reales.

---

## Notas de implementación

1. **Manejo del token:** al recibir `401`, intentar renovar con `/api/auth/refresh`. Si el refresh también falla, redirigir al login.

2. **Cronómetro del test:** el deadline real lo calcula el backend al hacer `start` (`startedAt + timeLimitMinutes`). Mostrar un countdown en el cliente con base en `timeLimitMinutes` desde que se llama `start`, pero no confiar en él para forzar el submit — el backend es la fuente de verdad. Si el candidato intenta hacer submit después del deadline, el DailyJob ya habrá marcado la submission como `Expired`.

3. **Flujo de test en dos pasos:** llamar `preview` para mostrar qué le espera al candidato. Solo llamar `start` cuando el candidato confirme explícitamente — ese POST registra `startedAt` y no hay vuelta atrás.

4. **Estado de la oferta:** cuando `status = "PendingPayment"`, mostrar el botón de pago y permitir edición libre. Una vez en `Open` o posterior, todo es solo lectura para la empresa.

5. **Email del candidato en matching:** el email real solo aparece cuando `stage = "Selected"`. En etapas anteriores viene `null`.

6. **Score del test en matching:** `testScore` y `testFeedback` son `null` mientras el candidato no haya enviado sus respuestas. Aparecen automáticamente en `GET /api/matching/{offerId}` en cuanto el candidato completa el test.

7. **Submit con evaluación pendiente:** si el backend devuelve `status = "Pending"` en la respuesta de submit (IA no disponible temporalmente), mostrar un mensaje amigable: "Tus respuestas fueron recibidas. Tu resultado estará disponible pronto." El backend reintenta la evaluación automáticamente cada 24h.

8. **Cancelación con advertencia (409):** al recibir 409 en `PATCH /cancel`, mostrar un diálogo con el `data.warning` y dos opciones: "Cancelar de todas formas" (llama `force-cancel`) y "Volver" (no hace nada).

9. **Rate limits:** al recibir `429`, esperar al menos 60 segundos antes de reintentar. No mostrar un spinner infinito — informar al usuario con un contador.

10. **Catálogo:** cargar categorías y skills una sola vez al iniciar la app y cachear — no cambian con frecuencia.
