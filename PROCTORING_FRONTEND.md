# Proctoring — Referencia para frontend

Integración del sistema de monitoreo comportamental con el backend .NET de MatchIQ.

---

## Visión general del flujo

```
[Candidato presiona "Comenzar prueba"]
        │
        ▼
POST /api/tests/{offerId}/candidate/start   ← .NET
        │  Retorna submissionId + preguntas del test
        ▼
POST /api/session/start                     ← Python (proctoring)
        │  Manda usuario_id + submission_id
        │  Retorna session_id
        ▼
[Candidato responde el test]
[Flutter envía frames cada 500ms al Python]
        │
        ▼
POST /api/tests/{testId}/submit             ← .NET
POST /api/session/end                       ← Python (proctoring)
        │  Ambos se llaman al terminar (orden indistinto)
```

---

## Cambio en el inicio del test (candidato)

### Endpoint

```
POST /api/tests/{offerId}/candidate/start
Authorization: Bearer <token>   (rol: Candidate)
```

### Respuesta — ANTES

```json
{
  "success": true,
  "data": {
    "id": 12,
    "offerId": 7,
    "title": "Test técnico — Backend Developer",
    "timeLimitMinutes": 45,
    "createdAt": "2026-06-28T14:00:00Z",
    "questions": [ ... ]
  }
}
```

### Respuesta — AHORA

```json
{
  "success": true,
  "data": {
    "submissionId": 456,
    "test": {
      "id": 12,
      "offerId": 7,
      "title": "Test técnico — Backend Developer",
      "timeLimitMinutes": 45,
      "createdAt": "2026-06-28T14:00:00Z",
      "questions": [ ... ]
    }
  }
}
```

**Diferencia clave:** la respuesta ahora tiene dos campos en la raíz — `submissionId` y `test`. El test con las preguntas vive dentro de `test`, no directamente en `data`.

### Qué debe hacer Flutter con `submissionId`

Guardarlo en memoria y pasarlo de inmediato al backend Python al iniciar el proctoring:

```dart
final response = await api.startTest(offerId);
final submissionId = response['submissionId'];
final test = response['test'];

// Iniciar proctoring con el submission_id
await proctoringService.iniciar(
  usuarioId: currentUser.id.toString(),
  submissionId: submissionId,
);
```

---

## Ajuste en el servicio de proctoring (Python)

Al llamar `POST /api/session/start` del backend Python, ahora se debe incluir `submission_id`:

```json
{
  "usuario_id": "123",
  "submission_id": 456
}
```

Sin `submission_id` el reporte no puede vincularse con la prueba técnica y la empresa no podrá verlo.

---

## Endpoint de reporte comportamental (empresa)

### Endpoint

```
GET /api/tests/submissions/{matchId}/proctoring
Authorization: Bearer <token>   (rol: Company)
```

El parámetro es el `matchId`, igual que en `GET /api/tests/submissions/{matchId}` (resultados del test).

### Respuesta exitosa

```json
{
  "success": true,
  "data": {
    "sessionId": "550e8400-e29b-41d4-a716-446655440000",
    "inicio": "2026-06-28T15:00:00Z",
    "fin": "2026-06-28T16:00:00Z",
    "totalFramesProcesados": 7200,
    "totalEventos": 3,
    "integrityScore": 60.0,
    "integritySummary": "Durante la sesión se detectaron tres incidentes relevantes: uso de un dispositivo adicional a los 10 minutos, ausencia prolongada de pantalla y presencia de una segunda persona hacia el final. El score de integridad de 60/100 indica riesgo moderado y se recomienda revisar los resultados con precaución.",
    "eventos": [
      {
        "tipo": "dispositivo_prohibido",
        "detalle": "Detectado: cell phone",
        "evidencia": "ruta/imagen.jpg",
        "timestamp": "2026-06-28T15:10:00Z"
      },
      {
        "tipo": "distraccion",
        "detalle": "Más de 10s fuera de pantalla",
        "evidencia": null,
        "timestamp": "2026-06-28T15:25:00Z"
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

### Descripción de campos

**Raíz del objeto:**

`sessionId` — UUID de la sesión de proctoring generada por el backend Python.

`inicio` — Timestamp de cuando el candidato comenzó el test (ISO 8601, UTC).

`fin` — Timestamp de cuando terminó la sesión. Puede ser `null` si la sesión no se cerró correctamente.

`totalFramesProcesados` — Cantidad de frames de cámara analizados por el modelo de IA durante la sesión.

`totalEventos` — Cantidad total de eventos sospechosos detectados. Útil para mostrar un contador rápido sin iterar los eventos.

`integrityScore` — Score numérico de 0 a 100 calculado con la fórmula determinística. Se descuenta por tipo de evento: `camara_cubierta` -30, `dispositivo_prohibido` -20, `segunda_persona` -20, `rostro_ausente` -15, `distraccion` -8, otros -5. Mínimo 0. Se calcula la primera vez que la empresa consulta el reporte y se cachea en BD.

`integritySummary` — Resumen ejecutivo en español generado por GPT-4o-mini analizando los eventos y el score. Se genera la primera vez que la empresa consulta el reporte y se cachea en BD (no se vuelve a llamar a la IA en consultas subsiguientes).

`eventos` — Lista de eventos ordenados cronológicamente.

---

**Cada objeto dentro de `eventos`:**

`tipo` — Categoría del evento detectado por el modelo. Valores conocidos:
- `"dispositivo_prohibido"` — se detectó un segundo dispositivo (celular, tablet, etc.)
- `"distraccion"` — el candidato estuvo fuera de pantalla por más de 10 segundos
- `"segunda_persona"` — se detectó otra persona en el frame
- `"camara_cubierta"` — la cámara fue tapada o bloqueada
- `"rostro_ausente"` — no se detectó ningún rostro en el frame por tiempo prolongado

`detalle` — Descripción textual del evento. Generada por el modelo Python. Puede ser `null`.

`evidencia` — Ruta o URL del frame capturado como prueba del evento. Puede ser `null` si el evento no generó captura.

`timestamp` — Momento exacto en que ocurrió el evento durante el test (ISO 8601, UTC).

---

### Qué mostrarle a la empresa en pantalla

Con este endpoint se puede construir una sección de "Reporte de integridad" dentro de la vista de resultados del candidato:

- Mostrar `integrityScore` como indicador visual (barra de progreso o círculo). Sugerencia de colores: 80-100 verde, 50-79 amarillo, 0-49 rojo.
- Mostrar `integritySummary` como párrafo de contexto bajo el score — es el análisis cualitativo de la IA.
- Si `totalEventos === 0`: mostrar un badge verde "Sin incidentes detectados"
- Si `totalEventos > 0`: mostrar un badge rojo/naranja con el número de incidentes y listar los eventos
- Para cada evento: mostrar el `tipo` como etiqueta, el `detalle` como descripción y el `timestamp` formateado
- Si `evidencia` no es `null`: mostrar un enlace o miniatura de la imagen capturada
- Mostrar `totalFramesProcesados` como indicador de cobertura del monitoreo

---

## Errores posibles

### Endpoint de inicio del test

| Código | Cuándo ocurre |
|--------|---------------|
| 401 | Token inválido o sin rol Candidate |
| 404 | Test o perfil de candidato no encontrado |
| 400 | El plazo del test expiró o ya fue enviado |

### Endpoint de reporte de proctoring

| Código | Cuándo ocurre |
|--------|---------------|
| 401 | Token inválido o sin rol Company |
| 403 | El match no pertenece a una oferta de esta empresa |
| 404 | Match no encontrado, submission no encontrada, o el candidato no tiene reporte de proctoring (la sesión Python no se inició o no se cerró) |
| 400 | El candidato todavía no completó el test |

---

## Orden de llamadas al terminar el test

Cuando el candidato termina (botón "Entregar" o tiempo agotado), Flutter debe llamar **ambos** endpoints:

```dart
// Ambos en paralelo — ninguno depende del otro
await Future.wait([
  api.submitAnswers(testId, answers),        // POST /api/tests/{testId}/submit  → .NET
  proctoringService.terminar(),              // POST /api/session/end             → Python
]);
```

Si `session/end` falla, las respuestas del test ya quedaron guardadas en .NET. El reporte de proctoring simplemente quedará sin `fin` en la BD — no bloquea la evaluación del test.
