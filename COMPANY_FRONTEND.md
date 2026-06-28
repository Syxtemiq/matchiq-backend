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
