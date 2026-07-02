# Información de contacto del candidato — Guía para frontend

Este documento cubre dos cosas relacionadas que cambiaron en el backend:

1. Se agregó **número de teléfono** al perfil del candidato (campo nuevo, obligatorio).
2. La empresa ahora puede ver el **contacto completo del candidato** (email, teléfono, GitHub,
   LinkedIn, foto) — pero **solo después de que el candidato completó el test**, no antes.

No hay endpoints nuevos — todo pasa por los mismos endpoints que ya usaban, con campos
adicionales en las respuestas.

---

## Parte 1 — Teléfono en el perfil del candidato

### `GET /api/candidate/profile` (rol: Candidate)

Ahora la respuesta trae un campo más:

```json
{
  "success": true,
  "data": {
    "userId": 12,
    "fullName": "Laura Gómez",
    "email": "laura@email.com",
    "experienceYears": 4,
    "seniority": "Mid",
    "englishLevel": "B2",
    "githubLink": "https://github.com/laurag",
    "linkedinUrl": "https://linkedin.com/in/laurag",
    "profilePhotoUrl": "https://...",
    "phoneNumber": "3",
    "profileCompleted": true,
    "categories": [ ... ],
    "skills": [ ... ]
  },
  "message": null
}
```

**Importante:** los candidatos que ya existían antes de este cambio tienen un `phoneNumber`
placeholder (`"1"`, `"2"`, `"3"`... un número secuencial de relleno, NO un teléfono real). El
frontend debe tratar esto como **dato pendiente de completar**, no como un teléfono válido —
sugerimos mostrar el campo vacío o resaltado si `phoneNumber.Length <= 2` (heurística simple para
detectar los placeholders), invitando al candidato a actualizarlo.

### `PUT /api/candidate/profile` (rol: Candidate)

El body ahora acepta `phoneNumber`:

```json
{
  "experienceYears": 4,
  "seniority": "mid",
  "englishLevel": "B2",
  "githubLink": "https://github.com/laurag",
  "linkedinUrl": "https://linkedin.com/in/laurag",
  "profilePhotoUrl": "https://...",
  "phoneNumber": "+57 300 1234567",
  "categoryIds": [1, 2],
  "skills": [{ "skillId": 5, "level": 4 }]
}
```

| Campo | Tipo | Obligatorio | Notas |
|---|---|---|---|
| `phoneNumber` | string | No (pero si lo omites, se queda con el valor placeholder) | Validado con regex: acepta dígitos, espacios, `+`, `-`, paréntesis, entre 7 y 20 caracteres. |

Error si el formato no pasa la validación:

```json
{ "success": false, "data": null, "message": "El número de teléfono no es válido." }
```

**Recomendación de UI:** agrega un input de teléfono en el formulario de "completar perfil" del
candidato — no es parte de `profileCompleted` (ese flag sigue dependiendo solo de
experiencia/seniority/inglés), pero conviene pedirlo desde ya para que cuando la empresa llegue a
verlo, no esté vacío ni con el placeholder.

---

## Parte 2 — La empresa ve el contacto completo, pero solo tras el test

### Regla de negocio (léela antes de tocar la UI)

Antes: la empresa solo veía el email del candidato después de **seleccionarlo**.
Ahora: la empresa ve **email + teléfono + GitHub + LinkedIn + foto** en cuanto el candidato
**completa y envía el test** (no hace falta seleccionarlo todavía). Antes de eso, esos campos
llegan en `null`, aunque el candidato ya haya aparecido en el ranking o incluso ya le hayan
enviado el test.

### `GET /api/matching/{offerId}` y `POST /api/matching/{offerId}/run` / `/reevaluate` (rol: Company)

`MatchResultDto` tiene estos campos nuevos (todos nullable, igual que `email` ya lo era):

```json
{
  "matchId": 42,
  "candidateId": 7,
  "fullName": "Laura Gómez",
  "email": null,
  "githubLink": null,
  "linkedinUrl": null,
  "profilePhotoUrl": null,
  "phoneNumber": null,
  "experienceYears": 4,
  "englishLevel": "B2",
  "matchPercentage": 87.5,
  "adjustedScore": 91.2,
  "stage": "TestSent",
  "matchedSkills": ["React", "TypeScript"],
  "testScore": null,
  "testFeedback": null
}
```

Y una vez que el candidato completa el test (submission evaluada por la IA), la misma respuesta
trae todo lleno:

```json
{
  "matchId": 42,
  "candidateId": 7,
  "fullName": "Laura Gómez",
  "email": "laura@email.com",
  "githubLink": "https://github.com/laurag",
  "linkedinUrl": "https://linkedin.com/in/laurag",
  "profilePhotoUrl": "https://...",
  "phoneNumber": "+57 300 1234567",
  "experienceYears": 4,
  "englishLevel": "B2",
  "matchPercentage": 87.5,
  "adjustedScore": 91.2,
  "stage": "TestCompleted",
  "matchedSkills": ["React", "TypeScript"],
  "testScore": 88.5,
  "testFeedback": "Excelente resolución del code challenge..."
}
```

**Regla simple para el frontend:** no necesitas calcular tú mismo si "ya se puede mostrar" — el
backend ya decide eso. Si `email` (o cualquiera de los otros 4 campos de contacto) viene `null`,
simplemente no lo muestres o muestra un placeholder tipo "Disponible cuando complete el test". Si
viene con valor, muéstralo. No uses `stage` para decidir esto — usa directamente si el campo es
`null` o no (más confiable, porque estos 5 campos se activan/desactivan todos juntos en el mismo
momento).

### `GET /api/tests/submissions/{matchId}` (rol: Company) — detalle del test ya resuelto

Este endpoint solo es alcanzable cuando el candidato **ya completó el test** (el backend lanza
`400` si no), así que aquí el contacto **siempre viene completo**, sin necesidad de chequear nulls:

```json
{
  "success": true,
  "data": {
    "matchId": 42,
    "candidateFullName": "Laura Gómez",
    "candidateEmail": "laura@email.com",
    "candidateGithubLink": "https://github.com/laurag",
    "candidateLinkedinUrl": "https://linkedin.com/in/laurag",
    "candidateProfilePhotoUrl": "https://...",
    "candidatePhoneNumber": "+57 300 1234567",
    "score": 88.5,
    "globalFeedback": "...",
    "status": "Evaluated",
    "submittedAt": "2026-07-02T10:00:00Z",
    "aiEvaluatedAt": "2026-07-02T10:05:00Z",
    "questions": [ ... ]
  },
  "message": null
}
```

Nota los nombres: aquí llevan el prefijo `candidate` (`candidateEmail`, `candidateGithubLink`,
etc.) porque este DTO es distinto al del ranking — no son el mismo shape, ojo al mapear.

---

## Resumen rápido para implementar

- [ ] Agregar input de teléfono al formulario de perfil del candidato (`PUT /api/candidate/profile`,
      campo `phoneNumber`).
- [ ] Mostrar el teléfono en `GET /api/candidate/profile` (ojo con los placeholders numéricos de
      candidatos viejos — invita a actualizarlo si el valor parece un placeholder).
- [ ] En la pantalla de ranking/resultados de la empresa (`MatchResultDto`): mostrar
      email/teléfono/GitHub/LinkedIn/foto solo si no vienen `null`; si vienen `null`, mostrar algo
      como "Disponible cuando el candidato complete el test".
- [ ] En la pantalla de detalle de submission (`CandidateSubmissionDetailDto`): mostrar el
      contacto directamente, siempre viene completo — pero usa los nombres con prefijo `candidate`.
- [ ] No hay que tocar la pantalla de "Seleccionar/Rechazar candidato" — esa sigue igual.
