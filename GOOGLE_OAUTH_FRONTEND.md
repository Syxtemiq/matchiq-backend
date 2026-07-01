# Registro y login con Google OAuth — Guía para frontend (Flutter Web)

Google se usa como **método alterno de registro/login**, junto al de email + contraseña. El
backend valida un **ID token de Google** (no un access token) y, con eso, crea el usuario si no
existe o vincula la cuenta si ya existe con ese email. Todo pasa por un único endpoint.

---

## 1. Qué necesitas antes de empezar

- El **Client ID de Google OAuth** (tipo "Web application"), configurado en el backend en
  `Google:ClientId` (`MatchIQ.API/appsettings.json`). **Debe ser el mismo Client ID en ambos
  lados** — el backend valida el `aud` (audience) del ID token contra ese valor exacto, y si no
  coincide el login falla. Pide el valor actual al equipo de backend (no lo copies de un commit
  viejo, puede rotar).
  - El Client ID **no es secreto** — está pensado para vivir en el código del frontend. El
    `Google:ClientSecret` sí es secreto y **no lo necesitas**, es solo para uso del backend.
- Paquete Flutter recomendado: [`google_sign_in`](https://pub.dev/packages/google_sign_in) (o
  `google_sign_in_web` si el proyecto ya separa por plataforma). Debe estar configurado para pedir
  **scopes de identidad básicos** (`email`, `profile`) — no hace falta nada más, el backend no usa
  el access token de Google, solo el ID token.
- Backend local corriendo en `http://localhost:5000` (confirma con el backend dev que el puerto no
  cambió).

---

## 2. Flujo completo

```
1. Usuario toca "Continuar con Google"
2. Flutter dispara el flujo de Google Sign-In → obtiene un idToken (JWT firmado por Google)
3. Flutter hace POST /api/auth/google con { idToken, role }
4. Backend valida el idToken contra Google:ClientId, busca/crea el usuario, devuelve tokens propios
5. Flutter guarda accessToken + refreshToken y navega según el estado del perfil
```

**Importante:** el `role` ("Candidate" o "Company") solo se usa si el usuario **es nuevo**. Si el
email de la cuenta de Google ya existe en MatchIQ (por registro normal o login previo con Google),
el backend **ignora silenciosamente** el `role` enviado y respeta el rol que el usuario ya tenía.
Por eso la pantalla de "Continuar con Google" debe dejar claro **antes de tocar el botón** si el
usuario se está registrando como candidato o como empresa (ver sección 5).

---

## 3. Endpoint: `POST /api/auth/google`

```
POST http://localhost:5000/api/auth/google
Content-Type: application/json
Sin Authorization (endpoint público)
Rate limit: 15 req/min por IP (política "auth-general")
```

### Request body

```json
{
  "idToken": "<id-token-que-devuelve-google-sign-in>",
  "role": "Candidate"
}
```

| Campo | Tipo | Obligatorio | Notas |
|---|---|---|---|
| `idToken` | string | Sí | El **ID token** JWT de Google, no el access token. |
| `role` | string | No | `"Candidate"` o `"Company"` (no sensible a mayúsculas). Si se omite, el backend asume `Candidate`. Solo aplica si el usuario es nuevo. |

### Respuesta exitosa `200`

```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "9f3c2a1b-...",
    "userId": 42,
    "role": "Candidate",
    "fullName": "Jane Doe",
    "emailVerified": true
  },
  "message": null
}
```

- `emailVerified` siempre viene en `true` para cuentas de Google (Google ya verificó el correo;
  no hay paso de código de verificación como en el registro normal).
- `accessToken` expira en **60 minutos**. Úsalo como `Authorization: Bearer <accessToken>` en el
  resto de los endpoints.
- `refreshToken` expira en **7 días**. Guárdalo para renovar el access token sin volver a pasar
  por Google (ver sección 4).
- Todos los nombres de campo llegan en **camelCase** (convención estándar de ASP.NET Core), y los
  enums (como `role`) llegan como **string**, no como número.

### Errores posibles

| HTTP | `message` (aprox.) | Causa / qué hacer en UI |
|---|---|---|
| `400` | `"Token de Google inválido: ..."` | El `idToken` expiró, es de otro Client ID, o está corrupto. Reintentar el flujo de Google Sign-In desde cero. |
| `400` | Error de validación del `role` | Solo se aceptan `Candidate`/`Company`; revisa que no estés mandando `Admin` u otro valor. |
| `401` | `"Usuario desactivado."` (aprox.) | La cuenta fue desactivada por un admin — mostrar mensaje y no dejar reintentar. |
| `429` | — | Rate limit (15/min por IP) — mostrar "intenta de nuevo en un momento". |

---

## 4. Refresh y logout (igual que el login normal)

El login con Google usa **el mismo sistema de tokens** que el login por email/contraseña, así que
el resto del ciclo de vida de sesión no cambia:

```
POST /api/auth/refresh
Body: { "refreshToken": "..." }
→ devuelve un AuthResponseDto nuevo (mismo shape que arriba), rotando el refresh token
```

```
POST /api/auth/logout      (requiere Authorization: Bearer <accessToken>)
Body: { "refreshToken": "..." }
→ revoca el refresh token en el servidor
```

Guarda ambos tokens en almacenamiento seguro del lado del cliente (en Flutter Web, `localStorage`
vía algo como `shared_preferences` con el plugin web, o mejor un wrapper que puedas migrar a
`flutter_secure_storage` si en el futuro hay build móvil).

---

## 5. Elegir el rol antes de llamar a Google

Como el `role` solo se respeta al crear el usuario, y Google no te deja pedirle el rol al usuario
en su propio flujo, la UI debe resolverlo **antes** de disparar `google_sign_in`:

- **Opción recomendada:** dos pantallas/botones separados desde el arranque — "Soy candidato" /
  "Soy empresa" — cada una dispara el mismo flujo de Google pero con `role` fijo según el botón
  que tocó el usuario. Esto es consistente con que el registro por email también obliga a elegir
  rol en `RegisterDto`.
- Si el usuario ya existe (login recurrente), no importa qué botón toque — el backend usa el rol
  guardado y listo.

No hay forma de cambiar el rol después de creada la cuenta a través de este endpoint.

---

## 6. Qué hacer después del login exitoso: perfil incompleto

Google **no crea automáticamente** un `CandidateProfile` ni un `CompanyProfile` — esas filas se
crean recién cuando el usuario guarda su perfil por primera vez. Es decir, un usuario nuevo por
Google llega con perfil vacío. Después de guardar los tokens, el flujo típico es:

1. Con el `accessToken`, llamar a `GET /api/candidate/profile` (si `role === "Candidate"`) o
   `GET /api/company/profile` (si `role === "Company"`).
2. Revisar el campo `profileCompleted` de la respuesta:
   - **Candidate**: `true` solo si ya tiene `experienceYears`, `seniority` y `englishLevel`
     guardados.
   - **Company**: `true` solo si ya tiene `companyName` guardado.
3. Si `profileCompleted === false`, redirigir a la pantalla de "completar perfil" en vez del
   dashboard normal.

Esto aplica tanto a usuarios nuevos por Google como a usuarios que se registraron por email pero
nunca completaron el perfil — es el mismo chequeo, no hay lógica especial para Google en este
punto.

---

## 7. Resumen rápido para implementar

- [ ] Configurar `google_sign_in` en Flutter Web con el **mismo Client ID** que usa el backend
      (`Google:ClientId`).
- [ ] Botón "Continuar con Google" en dos variantes (candidato / empresa) o con selector previo.
- [ ] Al completar el sign-in de Google, tomar el `idToken` (no el `accessToken` de Google).
- [ ] `POST /api/auth/google` con `{ idToken, role }`.
- [ ] Guardar `accessToken` + `refreshToken` de la respuesta.
- [ ] Llamar a `GET /api/{candidate|company}/profile` y revisar `profileCompleted` para decidir
      a dónde navegar.
- [ ] Implementar refresh silencioso con `POST /api/auth/refresh` cuando el `accessToken` expire
      (401 en cualquier endpoint protegido → intentar refresh una vez antes de forzar logout).
