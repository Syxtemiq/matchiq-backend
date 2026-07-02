# Idioma del test técnico — Guía para frontend

Al generar el test técnico de una oferta, la empresa ahora puede elegir si quiere que la IA
redacte las preguntas en **español** o en **inglés**. Es un campo opcional en el mismo endpoint
que ya usan para generar el test — no hay endpoints nuevos.

---

## Dónde va el selector

En la pantalla donde la empresa dispara la generación del test (después de crear/pagar la
oferta), agrega un selector simple — por ejemplo dos botones o un dropdown:

```
¿En qué idioma quieres el test?
( ) Español
( ) English
```

Si no seleccionan nada, el backend asume **español** por defecto (para no romper flujos
existentes), así que el selector puede venir pre-marcado en "Español".

---

## Endpoint: `POST /api/tests/{offerId}/generate`

(mismo cambio aplica a `POST /api/tests/{offerId}/regenerate`)

```
Authorization: Bearer <token>   (rol: Company)
```

### Request body

```json
{
  "timeLimitMinutes": 45,
  "testLanguage": "spanish"
}
```

| Campo | Tipo | Obligatorio | Notas |
|---|---|---|---|
| `timeLimitMinutes` | int | Sí | Como ya lo tenían. |
| `testLanguage` | string | No | `"spanish"` o `"english"` (no sensible a mayúsculas). Si se omite, el backend usa `"spanish"`. |

### Errores

| HTTP | `message` | Causa |
|---|---|---|
| `400` | `"El idioma del test debe ser spanish o english."` | Mandaste un valor distinto a `spanish`/`english` (validación del DTO). |
| `400` | `"Idioma de test inválido: '...'"` | Mismo caso, validado también del lado del servicio. No debería pasar si el frontend valida antes de mandar. |

---

## Respuesta — el test ya trae su idioma

El `TestDto` (respuesta de `generate`, `regenerate` y `GET /api/tests/{offerId}`) ahora incluye
el campo `testLanguage`:

```json
{
  "success": true,
  "data": {
    "id": 12,
    "offerId": 7,
    "title": "Backend Developer Technical Assessment",
    "timeLimitMinutes": 45,
    "testLanguage": "English",
    "createdAt": "2026-07-01T20:00:00Z",
    "questions": [ ... ]
  },
  "message": null
}
```

Nota: en la respuesta viene con mayúscula inicial (`"Spanish"` / `"English"`, es el `.ToString()`
del enum de C#), a diferencia del request donde se manda en minúscula. Vale la pena mostrarlo en
la UI de la oferta (ej. una etiqueta "Test en inglés") para que la empresa recuerde en qué idioma
quedó generado, sobre todo si después vuelve a la pantalla de edición de preguntas.

---

## Qué NO cambia

- **La edición de preguntas por chat** (el chat de "editar pregunta con IA") no necesita que le
  mandes el idioma — el backend ya sabe en qué idioma se generó el test (queda guardado) y
  mantiene esa consistencia automáticamente. No hay cambios en ese endpoint desde el frontend.
- **El idioma de la oferta en sí** (título, descripción que escribe la empresa) no se toca —
  esto es únicamente sobre las preguntas del test generado por IA. La empresa sigue escribiendo
  la oferta en el idioma que quiera, independientemente del idioma del test.
- Una vez generado el test, el idioma queda fijo para ese test — solo se puede volver a elegir
  si la empresa usa "Regenerar" (y solo está disponible mientras la oferta sigue en
  `PendingPayment`, misma regla que ya existía).

---

## Resumen rápido para implementar

- [ ] Agregar selector Español/English en la pantalla de generación de test (default: Español).
- [ ] Mandar `testLanguage: "spanish"` o `"english"` en el body de `POST /generate` (y
      `/regenerate` si aplica).
- [ ] Mostrar `data.testLanguage` en la UI de la oferta/test para que quede claro en qué idioma
      se generó.
- [ ] No se requiere ningún cambio en la pantalla de chat de edición de preguntas.
