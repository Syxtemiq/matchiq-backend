# MatchIQ — Integración de Pagos (Stripe)

> Documento para el equipo de frontend.  
> Proveedor activo: **Stripe Checkout** (modo sesión, pago único).  
> Todas las rutas requieren `Authorization: Bearer <token>` salvo que se indique lo contrario.

---

## Flujo completo

```
[Empresa crea oferta]
        ↓
POST /api/payments/create-checkout?offerId=X
        ↓
    Backend crea sesión en Stripe
        ↓
  Respuesta: { url: "https://checkout.stripe.com/..." }
        ↓
    Frontend redirige al usuario a esa URL
        ↓
  ┌─────────────────────────────────────┐
  │  Usuario completa pago en Stripe    │
  └─────────────────────────────────────┘
        ↓                      ↓
   Stripe redirige         Stripe envía
   a SuccessUrl            webhook al backend
        ↓                      ↓
  Frontend lee              Backend activa
  session_id del URL        la oferta automáticamente
        ↓
POST /api/payments/verify-session?sessionId=cs_...
        ↓
  Backend confirma y activa oferta (idempotente)
        ↓
  Frontend muestra oferta como "Open"
```

---

## Endpoints

### 1. `POST /api/payments/create-checkout`

**Auth:** Company  
**Query param:** `offerId` (integer)

Crea una sesión de Stripe Checkout y devuelve la URL a la que debes redirigir al usuario.

**Respuesta normal — 200 OK:**
```json
{
  "success": true,
  "data": {
    "url": "https://checkout.stripe.com/c/pay/cs_test_..."
  }
}
```

**Respuesta cuando el pago ya fue procesado — 200 OK:**

Puede ocurrir si el usuario pagó pero el frontend nunca llamó a `verify-session`.
El backend detecta el pago previo, activa la oferta y responde así:

```json
{
  "success": true,
  "message": "El pago ya fue procesado. La oferta ha sido activada.",
  "data": {
    "url": null,
    "activated": true
  }
}
```

> **Acción del frontend:** cuando `data.activated == true`, no redirigir a Stripe.
> Navegar directamente a la vista de detalle de la oferta y refrescar su estado.

**Errores posibles:**

| HTTP | `message` | Qué hacer |
|------|-----------|-----------|
| 400 | "La oferta no está pendiente de pago." | La oferta ya fue activada (o está en otro estado). Refrescar el estado de la oferta. |
| 400 | "La oferta no puede ser modificada..." | No aplica aquí, es de otro endpoint. |
| 404 | "Oferta no encontrada." | offerId incorrecto o no pertenece a la empresa. |
| 429 | — | Rate limit: máx 5 intentos por 5 minutos por usuario. Mostrar mensaje de espera. |

---

### 2. Redirección de Stripe → SuccessUrl y CancelUrl

Stripe redirige al usuario de vuelta al frontend cuando termina el checkout.

**SuccessUrl** (pago completado o intentado):
```
http://[FRONTEND]/payment-result?offerId={offerId}&session_id={CHECKOUT_SESSION_ID}
```

**CancelUrl** (usuario cerró o canceló):
```
http://[FRONTEND]/payment-result?offerId={offerId}&success=false
```

**Lógica en la página `/payment-result`:**

```dart
// Pseudo-código — adaptar al framework del frontend
final params = Uri.base.queryParameters;

if (params.containsKey('session_id')) {
  // Usuario completó el flujo en Stripe (puede ser pago exitoso o fallido)
  // SIEMPRE llamar verify-session para confirmar
  await verifySession(sessionId: params['session_id']!, offerId: params['offerId']!);

} else if (params['success'] == 'false') {
  // Usuario canceló en Stripe — oferta sigue como PendingPayment
  mostrarMensajeCancelacion();
  navegarADetalleOferta(offerId: params['offerId']!);
}
```

> **Importante:** La redirección a `SuccessUrl` ocurre cuando el usuario *completa* el flujo de Stripe,
> no necesariamente cuando el pago fue aprobado. Siempre verificar con `verify-session`.

---

### 3. `POST /api/payments/verify-session`

**Auth:** Company  
**Query param:** `sessionId` (string — el valor de `session_id` del URL de retorno)

Verifica con Stripe que el pago fue aprobado y activa la oferta. Es **idempotente**: puede llamarse múltiples veces sin efecto secundario.

**Pago aprobado — 200 OK:**
```json
{
  "success": true,
  "message": "Pago verificado. Oferta activada.",
  "data": {
    "activated": true
  }
}
```

**Pago aún no procesado — 200 OK:**
```json
{
  "success": true,
  "message": "El pago aún no ha sido procesado.",
  "data": {
    "activated": false
  }
}
```

> Si `activated == false`, puedes reintentar después de unos segundos (Stripe puede demorar) o
> redirigir al usuario a la pantalla de la oferta con estado `PendingPayment`.

**Errores posibles:**

| HTTP | `message` | Qué hacer |
|------|-----------|-----------|
| 400 | "sessionId es requerido." | Bug en el frontend — revisar lectura del query param. |
| 404 | "No se encontró el registro de pago para esta sesión." | sessionId no coincide con ninguna oferta de esta empresa. |
| 401 | — | Token expirado o empresa diferente. |

---

### 4. `POST /api/payments/webhook` *(solo backend — no llamar desde el frontend)*

Stripe lo llama automáticamente cuando una transacción se actualiza. El backend activa la oferta internamente. El frontend no debe interactuar con este endpoint.

---

## Diagrama de estados de la oferta

```
PendingPayment  ──(pago confirmado)──→  Open  ──(test enviado)──→  TestSent
                                          ↓
                                       Cancelled / Expired
```

El frontend debe refrescar el estado de la oferta (`GET /api/offers/{id}`) después de:
- Recibir `data.activated == true` de `create-checkout`
- Recibir `data.activated == true` de `verify-session`

---

## Configuración requerida en el backend

En `appsettings.json` (o variables de entorno en producción):

```json
"Stripe": {
  "PublicKey":      "pk_test_...",
  "PrivateKey":     "sk_test_...",     ← nunca exponer al frontend
  "WebhookSecret":  "whsec_...",       ← se obtiene en el dashboard de Stripe
  "SuccessUrl":     "https://[FRONTEND]/payment-result",
  "CancelUrl":      "https://[FRONTEND]/payment-result"
}
```

> El `WebhookSecret` actualmente está vacío en `appsettings.json`.
> En producción debe configurarse desde el dashboard de Stripe → Webhooks → tu endpoint → "Signing secret".
> Sin él, el backend acepta webhooks sin verificar firma (solo permitido en desarrollo).

---

## Llave pública de Stripe (para el frontend si es necesario)

```
pk_test_51Tn2QsJE7nbMdjeEPVmigfuXwqIksXp0f6cRSEmGmYbyVBjGLTSGQU47uNai4FAfZ7Kn614bi9VsfZ4FjcIBqThK00u5X8GgLJ
```

Esta es la llave de **sandbox**. Cambiarla por la llave de producción al hacer el deploy real.

---

## Resumen del bug que había y cómo afectaba al frontend

**Síntoma:** había que pagar dos veces para que la oferta quedara activa.

**Causa (ya corregida en el backend):**
1. Usuario pagaba → Stripe marcaba la sesión como `complete + paid`
2. El webhook no llegaba o `verify-session` no se llamaba → oferta seguía como `PendingPayment`
3. Frontend mostraba el botón de pago de nuevo
4. Usuario llamaba `create-checkout` → backend detectaba la sesión ya pagada pero crasheaba internamente al intentar activar (`JobOffer` no estaba cargado)
5. El crash se tragaba silenciosamente → se creaba una **nueva sesión de Stripe** → usuario pagaba una segunda vez

**Corrección en el backend:** el query ahora carga `JobOffer` correctamente. Cuando detecta un pago previo ya realizado, activa la oferta y responde con `data.activated = true` (200 OK) en lugar de crear una nueva sesión.

**Lo que el frontend debe garantizar:**
- Siempre llamar `verify-session` después de que Stripe redirija a `SuccessUrl`
- Si `create-checkout` responde con `data.activated == true`, no redirigir a Stripe
- Después de cualquier activación, refrescar el estado de la oferta antes de mostrar la UI

---

## Checklist de implementación

- [ ] Ruta `/payment-result` implementada en el router del frontend
- [ ] Lee `session_id` del query param al llegar de Stripe
- [ ] Llama `verify-session` cuando `session_id` está presente
- [ ] Maneja `data.activated == true` desde `create-checkout` (sin redirigir a Stripe)
- [ ] Maneja `data.activated == false` desde `verify-session` (pago pendiente)
- [ ] Maneja `success=false` del CancelUrl (usuario canceló)
- [ ] Refresca estado de la oferta (`GET /api/offers/{id}`) después de activación
- [ ] Muestra estado de la oferta (`PendingPayment`, `Open`, etc.) correctamente
