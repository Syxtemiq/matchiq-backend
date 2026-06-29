# Endpoints de empresa — Referencia para frontend

---

# Dashboard de métricas

## Endpoint

```
GET /api/company/dashboard
Authorization: Bearer <token>   (rol: Company)
```

Retorna métricas agregadas de toda la actividad de la empresa en la plataforma. No recibe parámetros.

---

## Respuesta exitosa

```json
{
  "offers": {
    "total": 10,
    "open": 3,
    "testSent": 2,
    "completed": 4,
    "cancelled": 1,
    "expired": 0,
    "pendingPayment": 0
  },
  "matches": {
    "total": 87,
    "testSent": 40,
    "testCompleted": 31,
    "selected": 8,
    "rejected": 15,
    "selectionRate": 20.0
  },
  "tests": {
    "sent": 40,
    "completed": 31,
    "evaluated": 28,
    "expired": 3,
    "completionRate": 77.5,
    "averageScore": 72.4
  }
}
```

### Bloque `offers` — estado de las ofertas

`total` — Cantidad total de ofertas creadas por la empresa.

`open` — Ofertas activas acumulando candidatos (pagadas, sin haber enviado el test todavía).

`testSent` — Ofertas en las que ya se envió el test a los candidatos seleccionados.

`completed` — Ofertas finalizadas (todas las submissions evaluadas o expiradas).

`cancelled` — Ofertas canceladas manualmente.

`expired` — Ofertas que vencieron por tiempo sin haber enviado el test.

`pendingPayment` — Ofertas creadas pero sin pago confirmado aún.

---

### Bloque `matches` — candidatos

`total` — Total de candidatos que hicieron match con alguna oferta de la empresa.

`testSent` — Cuántos de esos candidatos recibieron el test.

`testCompleted` — Cuántos completaron y enviaron el test.

`selected` — Cuántos fueron seleccionados por la empresa.

`rejected` — Cuántos fueron descartados.

`selectionRate` — Porcentaje de candidatos seleccionados sobre el total que recibieron el test (ej: `20.0` = 20%). Es `0` si nadie recibió el test aún.

---

### Bloque `tests` — desempeño de los tests

`sent` — Total de tests enviados a candidatos (equivale a `matches.testSent`).

`completed` — Cuántos candidatos completaron y enviaron sus respuestas.

`evaluated` — Cuántos de esos fueron evaluados por la IA (tienen score disponible).

`expired` — Cuántos candidatos dejaron vencer el tiempo sin responder.

`completionRate` — Porcentaje de candidatos que completaron el test sobre los que lo recibieron (ej: `77.5` = 77.5%). Es `0` si nadie recibió el test aún.

`averageScore` — Promedio de puntaje (0–100) de todos los tests evaluados por IA. Es `null` si todavía no hay ninguno evaluado.

---

### Qué mostrarle a la empresa en pantalla

Con este endpoint se puede construir un dashboard de inicio con:

- Tarjetas de estado de ofertas: activas, enviadas, completadas, canceladas
- Un número grande con total de candidatos y cuántos están en cada etapa del proceso
- Tasa de completación del test como indicador de interés de los candidatos
- Tasa de selección como indicador de calidad del proceso
- Puntaje promedio como referencia del nivel general de los candidatos evaluados
- Si `averageScore` es null o `evaluated` es 0, mostrar un estado vacío ("aún no hay evaluaciones")

---

---

# Reporte de empresa (descarga Excel)

## Endpoint

```
GET /api/company/report
Authorization: Bearer <token>   (rol: Company)
```

Devuelve un archivo `.xlsx` con dos hojas con toda la actividad de la empresa. El frontend debe tratarlo como descarga de archivo, no como JSON.

### Cómo manejar la descarga en el frontend

```js
const response = await fetch('/api/company/report', {
  headers: { Authorization: `Bearer ${token}` }
});
const blob = await response.blob();
const url  = URL.createObjectURL(blob);
const a    = document.createElement('a');
a.href     = url;
a.download = 'reporte-empresa.xlsx';
a.click();
URL.revokeObjectURL(url);
```

### Hoja 1 — Mis Ofertas

Una fila por cada oferta de la empresa con:

`Título` — nombre de la oferta.
`Estado` — Open, TestSent, Completed, Cancelled, Expired, PendingPayment.
`Modalidad` — Remote, Hybrid, OnSite, etc.
`Salario (COP)` — salario publicado, o "—" si no se especificó.
`Posiciones` — cuántas vacantes tenía la oferta.
`Tier` — plan contratado para esa oferta.
`Candidatos a testear` — cuántos candidatos estaban programados para recibir el test.
`Test enviado el` — fecha en que se envió el test, o "—".
`Creada el` — fecha de creación de la oferta.
`Total matches` — cuántos candidatos hicieron match.
`Tests enviados` — cuántos recibieron el test.
`Tests completados` — cuántos lo respondieron.
`Evaluados por IA` — cuántos tienen puntaje de IA.
`Seleccionados` — cuántos fueron seleccionados.
`Puntaje promedio` — promedio de score de los evaluados, o "—" si ninguno fue evaluado aún.

### Hoja 2 — Pipeline de Candidatos

Una fila por cada candidato que hizo match con alguna oferta de la empresa, con:

`Candidato` — nombre completo.
`Email` — email del candidato.
`Oferta` — a qué oferta aplicó.
`Etapa` — etapa actual del match (Matched, TestSent, TestCompleted, Selected, Rejected).
`Puntaje IA` — puntaje del test (0–100), o "—" si no fue evaluado.
`Feedback global IA` — resumen de la IA sobre el desempeño del candidato.
`Test enviado el` — fecha y hora en que el candidato envió sus respuestas.
`IA evaluó el` — fecha y hora en que la IA completó la evaluación.

---

---

# Resultados de test de candidato (vista empresa)

## Endpoint

```
GET /api/tests/submissions/{matchId}
Authorization: Bearer <token>   (rol: Company)
```

El parámetro es el **matchId**, no el offerId. La empresa solo puede ver la submission de matches que pertenezcan a sus propias ofertas. Si el candidato todavía no completó el test, el backend retorna error.

---

## Respuesta exitosa

```json
{
  "matchId": 42,
  "candidateFullName": "Juan Pérez",
  "score": 87.5,
  "globalFeedback": "El candidato demostró buen manejo de estructuras de datos...",
  "status": "Evaluated",
  "submittedAt": "2026-06-25T14:32:00Z",
  "aiEvaluatedAt": "2026-06-25T14:33:10Z",
  "questions": [ ... ]
}
```

### Campos del objeto raíz

`matchId` — ID del match al que pertenece esta submission.

`candidateFullName` — Nombre completo del candidato tal como está registrado en su perfil.

`score` — Puntaje total del test, de 0 a 100. Puede ser `null` si la IA aún no evaluó.

`globalFeedback` — Texto libre generado por la IA con un resumen general del desempeño del candidato. Puede ser `null` si no se evaluó aún.

`status` — Estado de la submission. Los valores posibles son:
- `"Pending"` — el candidato envió pero la IA todavía no evaluó
- `"Evaluated"` — la IA ya evaluó y el score está disponible
- `"Failed"` — la evaluación de la IA falló (se reintentará automáticamente)

`submittedAt` — Fecha y hora en que el candidato envió sus respuestas (ISO 8601, UTC).

`aiEvaluatedAt` — Fecha y hora en que la IA completó la evaluación. `null` si todavía no ocurrió.

`questions` — Lista de preguntas con las respuestas del candidato y la evaluación de la IA, en el orden en que aparecieron en el test.

---

### Cada objeto dentro de `questions`

```json
{
  "questionId": 7,
  "orderIndex": 1,
  "questionType": "MultipleChoice",
  "questionText": "¿Cuál es la complejidad de una búsqueda en un hashmap?",
  "options": {
    "A": "O(n)",
    "B": "O(log n)",
    "C": "O(1)",
    "D": "O(n²)"
  },
  "correctAnswer": "C",
  "selectedOption": "C",
  "functionSignature": null,
  "expectedBehavior": null,
  "codeSubmitted": null,
  "isCorrect": true,
  "aiFeedback": "Correcto. El candidato identificó correctamente la complejidad O(1) promedio."
}
```

`questionId` — ID interno de la pregunta.

`orderIndex` — Posición de la pregunta en el test (empieza en 1).

`questionType` — Tipo de pregunta. Solo hay dos valores: `"MultipleChoice"` o `"CodeChallenge"`.

`questionText` — El enunciado de la pregunta.

`isCorrect` — `true` si la IA la marcó como correcta, `false` si no. `null` si todavía no fue evaluada.

`aiFeedback` — Explicación de la IA sobre la respuesta del candidato en esa pregunta específica. `null` si no fue evaluada.

---

#### Campos exclusivos de MultipleChoice

`options` — Diccionario con las opciones disponibles. Las claves son `"A"`, `"B"`, `"C"`, `"D"` y los valores son el texto de cada opción.

`correctAnswer` — La clave de la opción correcta (ej: `"C"`).

`selectedOption` — La clave que eligió el candidato (ej: `"C"`). `null` si no respondió.

Los campos `functionSignature`, `expectedBehavior` y `codeSubmitted` vienen `null` en este tipo.

---

#### Campos exclusivos de CodeChallenge

`functionSignature` — La firma de la función que el candidato debía implementar (ej: `"int sumArray(int[] nums)"`).

`expectedBehavior` — Descripción textual de lo que debía hacer la función.

`codeSubmitted` — El código que escribió y envió el candidato. `null` si no respondió.

Los campos `options`, `correctAnswer` y `selectedOption` vienen `null` en este tipo.

---

## Errores posibles

| Código | Cuándo ocurre |
|--------|---------------|
| 401 | Token inválido o sin rol Company |
| 403 | El match no pertenece a una oferta de esta empresa |
| 404 | Match no encontrado, o el candidato nunca inició el test |
| 400 | El candidato inició el test pero no lo completó todavía |

---

## Qué mostrarle a la empresa en pantalla

Con estos datos el frontend puede construir una vista que incluya:

- Nombre del candidato y puntaje total destacado
- Estado de la evaluación (si es `Pending` o `Failed`, mostrar un mensaje de "evaluación en proceso")
- Feedback global de la IA como resumen ejecutivo
- Lista de preguntas donde para cada una se muestre el enunciado, lo que respondió el candidato, si estuvo correcto o no, y el feedback de la IA
- Para MultipleChoice: mostrar las opciones con la elegida resaltada en verde o rojo según `isCorrect`, e indicar cuál era la correcta si falló
- Para CodeChallenge: mostrar el código enviado en un bloque de código, junto con el feedback de la IA debajo

---

---

# Reporte de proctoring (comportamiento durante el test)

## Endpoint

```
GET /api/tests/submissions/{matchId}/proctoring
Authorization: Bearer <token>   (rol: Company)
```

El parámetro es el mismo `matchId` que en `GET /api/tests/submissions/{matchId}`. Solo disponible después de que el candidato completó el test.

---

## Respuesta exitosa

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
      }
    ]
  }
}
```

### Campos clave

`integrityScore` — Score de 0 a 100. Se calcula automáticamente la primera vez que la empresa consulta el reporte y se guarda en BD. Fórmula: `max(0, 100 - penalties)`. Penalización por tipo: `camara_cubierta` -30, `dispositivo_prohibido` -20, `segunda_persona` -20, `rostro_ausente` -15, `distraccion` -8, otros -5.

`integritySummary` — Párrafo en español generado por IA (GPT-4o-mini) que resume los incidentes y la confiabilidad del test. Se genera la primera vez y se cachea — las consultas posteriores no consumen tokens.

`eventos[].tipo` — Valores posibles: `"dispositivo_prohibido"`, `"distraccion"`, `"segunda_persona"`, `"camara_cubierta"`, `"rostro_ausente"`.

`eventos[].evidencia` — Ruta o URL del frame capturado como evidencia. Puede ser `null`.

---

## Qué mostrarle a la empresa

- Mostrar `integrityScore` como indicador visual (barra o círculo). Sugerencia: 80-100 verde, 50-79 amarillo, 0-49 rojo.
- Mostrar `integritySummary` como análisis ejecutivo bajo el score.
- Si `totalEventos === 0`: badge verde "Sin incidentes detectados".
- Si `totalEventos > 0`: badge rojo/naranja con el conteo de incidentes y lista de eventos.
- Si `evidencia` no es `null`: mostrar miniatura o enlace de la imagen capturada.

---

## Errores posibles

| Código | Cuándo ocurre |
|--------|---------------|
| 401 | Token inválido o sin rol Company |
| 403 | El match no pertenece a una oferta de esta empresa |
| 400 | El candidato todavía no completó el test |
| 404 | Match, submission o sesión de proctoring no encontrada |
