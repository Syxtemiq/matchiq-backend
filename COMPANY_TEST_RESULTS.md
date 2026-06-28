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
