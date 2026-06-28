# Endpoints de administrador — Referencia para frontend

Todos los endpoints requieren rol `Admin`.

```
Authorization: Bearer <token>   (rol: Admin)
```

---

# Estadísticas generales de la plataforma

## Endpoint

```
GET /api/admin/stats
```

Retorna un snapshot completo del estado de la plataforma en tiempo real.

## Respuesta

```json
{
  "totalCandidates": 320,
  "totalCompanies": 45,
  "usersRegisteredLast30Days": 28,

  "totalOffers": 90,
  "offersCreatedLast30Days": 12,
  "offersActive": 15,
  "offersCompleted": 40,
  "offersCancelled": 8,
  "offersExpired": 3,
  "offersPendingPayment": 2,
  "offersByStatus": {
    "Open": 15,
    "TestSent": 22,
    "Completed": 40,
    "Cancelled": 8,
    "Expired": 3,
    "PendingPayment": 2
  },

  "totalMatches": 1840,
  "matchesSelected": 95,
  "matchesRejected": 120,
  "matchesTestSent": 410,
  "matchesTestCompleted": 310,

  "activeTests": 12,
  "pendingSubmissions": 45,
  "submissionsEvaluated": 280,
  "submissionsExpired": 30,
  "averageTestScore": 71.4,

  "totalRevenueCop": 18500000,
  "paymentsCompleted": 88,
  "paymentsPending": 4,

  "testCompletionRate": 90.3,
  "selectionRate": 18.2
}
```

### Campos clave

`totalCandidates` / `totalCompanies` — usuarios activos por rol en la plataforma.

`usersRegisteredLast30Days` — nuevos registros en los últimos 30 días (candidatos + empresas).

`offersActive` — ofertas en estado `Open` (pagadas, acumulando matches, sin enviar test aún).

`offersByStatus` — desglose completo de todas las ofertas por estado. Los estados posibles son `Open`, `TestSent`, `Completed`, `Cancelled`, `Expired`, `PendingPayment`.

`matchesTestSent` — candidatos a los que se les envió el test.

`matchesTestCompleted` — candidatos que completaron y enviaron sus respuestas.

`matchesSelected` — candidatos que fueron seleccionados por alguna empresa.

`activeTests` — tests distintos que tienen al menos una submission pendiente de respuesta en este momento.

`pendingSubmissions` — submissions individuales que el candidato no ha completado aún.

`submissionsEvaluated` — submissions ya evaluadas por la IA (tienen puntaje).

`submissionsExpired` — candidatos que dejaron vencer el tiempo sin responder.

`averageTestScore` — puntaje promedio de todos los tests evaluados en la plataforma (0–100).

`totalRevenueCop` — suma de todos los pagos completados en COP.

`testCompletionRate` — porcentaje de submissions evaluadas sobre el total de submissions finalizadas (evaluadas + expiradas).

`selectionRate` — porcentaje de candidatos seleccionados sobre los que completaron el test.

---

### Qué mostrarle al admin en pantalla

Con este endpoint se puede construir un dashboard ejecutivo con:

- Tarjetas de usuarios: total candidatos, total empresas, nuevos este mes
- Embudo de conversión: matches → test enviado → completado → seleccionado
- Estado de ofertas con gráfico de torta o barras usando `offersByStatus`
- Revenue total y pagos completados vs pendientes
- Tasa de completación de tests y tasa de selección como KPIs destacados
- Puntaje promedio de plataforma como indicador de calidad

---

# Gestión de usuarios

## Listar todos los usuarios

```
GET /api/admin/users
GET /api/admin/users?role=Candidate
GET /api/admin/users?role=Company
GET /api/admin/users?isActive=true
GET /api/admin/users?role=Company&isActive=false
```

Parámetros opcionales de query: `role` (Candidate, Company, Admin) y `isActive` (true/false). Se pueden combinar.

### Respuesta — lista de usuarios

```json
[
  {
    "id": 12,
    "email": "empresa@ejemplo.com",
    "fullName": "Juan Pérez",
    "cedula": "123456789",
    "role": "Company",
    "isActive": true,
    "emailVerified": true,
    "createdAt": "2026-01-15T10:30:00Z",
    "profileName": "Tech SAS"
  }
]
```

`profileName` — si el usuario es Company, muestra el nombre de la empresa. En Candidate y Admin viene `null`.

---

## Ver un usuario por ID

```
GET /api/admin/users/{userId}
```

Retorna el mismo objeto que en el listado pero para un solo usuario.

---

## Activar / desactivar cuenta

```
PATCH /api/admin/users/{userId}/toggle-status
```

Invierte el estado activo del usuario. Si estaba activo lo desactiva, y viceversa. No aplica a cuentas Admin.

Retorna el usuario actualizado con el nuevo `isActive`.

---

## Crear administrador

```
POST /api/admin/users
```

```json
{
  "email": "nuevo.admin@matchiq.com",
  "fullName": "Nombre Completo",
  "cedula": "987654321",
  "password": "Contraseña123!",
  "confirmPassword": "Contraseña123!"
}
```

Crea un nuevo usuario con rol Admin. El email ya queda verificado automáticamente.

---

## Eliminar usuario

```
DELETE /api/admin/users/{userId}
```

Elimina permanentemente al usuario. No aplica a cuentas Admin. Usar con confirmación en el frontend.

---

# Reporte de plataforma (descarga Excel)

## Endpoint

```
GET /api/admin/report
```

Devuelve un archivo `.xlsx` con tres hojas. El frontend debe tratarlo como descarga de archivo, no como JSON.

### Cómo manejar la descarga en el frontend

```js
const response = await fetch('/api/admin/report', {
  headers: { Authorization: `Bearer ${token}` }
});
const blob = await response.blob();
const url  = URL.createObjectURL(blob);
const a    = document.createElement('a');
a.href     = url;
a.download = 'reporte-admin.xlsx';
a.click();
URL.revokeObjectURL(url);
```

### Hoja 1 — Resumen Plataforma

Lista de KPIs organizados en dos columnas: **Indicador** y **Valor**. Incluye bloques de usuarios, ofertas, matching, tests e ingresos. Pensado para imprimirse o adjuntarse en reportes ejecutivos.

### Hoja 2 — Empresas

Una fila por empresa registrada en la plataforma con:

`Empresa` — nombre de la empresa.
`Email` — email del usuario.
`Miembro desde` — fecha de creación del perfil.
`Total ofertas` — cuántas ofertas publicó.
`Activas` — ofertas en estado Open o TestSent.
`Completadas` — ofertas finalizadas.
`Canceladas` — ofertas canceladas manualmente.
`Total matches` — cuántos candidatos hicieron match con sus ofertas.
`Seleccionados` — cuántos candidatos fueron seleccionados.
`Total pagado (COP)` — suma de todos sus pagos completados.

### Hoja 3 — Pagos

Una fila por pago registrado en el sistema con:

`Empresa` — nombre de la empresa.
`Email empresa` — email del usuario empresa.
`Oferta` — título de la oferta asociada al pago.
`Tier` — plan contratado.
`Monto (COP)` — valor pagado.
`Estado` — `Succeeded`, `Pending`, u otros estados de Stripe.
`Pagado el` — fecha y hora del pago confirmado. "Pendiente" si no se confirmó.
`Transaction ID` — ID de la transacción en Stripe para trazabilidad.

---

# Errores posibles (todos los endpoints admin)

| Código | Cuándo ocurre |
|--------|---------------|
| 401 | Token inválido o expirado |
| 403 | El token no tiene rol Admin |
| 404 | Usuario o recurso no encontrado |
| 400 | Datos inválidos (contraseñas no coinciden, email duplicado, etc.) |
