# Analytics de mercado — Referencia para frontend

Dos endpoints que exponen inteligencia de demanda/oferta de skills basada en los datos reales
de la plataforma. El primero es público (sirve para cualquier pantalla informativa). El segundo
es exclusivo para candidatos autenticados y cruza los datos del mercado con el perfil propio.

---

# 1. Mercado general

## Endpoint

```
GET /api/analytics/market
Sin Authorization (público)
```

No recibe parámetros. Devuelve las top skills más pedidas en ofertas, las más comunes en
candidatos, y las combinaciones de skills que las empresas más buscan juntas.

---

## Respuesta exitosa

```json
{
  "success": true,
  "data": {
    "topDemand": [
      { "skillName": "React",      "categoryName": "FrontEnd", "offerCount": 23 },
      { "skillName": "Docker",     "categoryName": "DevOps",   "offerCount": 18 },
      { "skillName": "TypeScript", "categoryName": "FrontEnd", "offerCount": 15 },
      { "skillName": "Node.js",    "categoryName": "BackEnd",  "offerCount": 14 },
      { "skillName": "PostgreSQL", "categoryName": "Databases","offerCount": 12 }
    ],
    "topSupply": [
      { "skillName": "JavaScript", "categoryName": "FrontEnd", "candidateCount": 41 },
      { "skillName": "React",      "categoryName": "FrontEnd", "candidateCount": 37 },
      { "skillName": "Python",     "categoryName": "BackEnd",  "candidateCount": 29 },
      { "skillName": "Docker",     "categoryName": "DevOps",   "candidateCount": 22 },
      { "skillName": "PostgreSQL", "categoryName": "Databases","candidateCount": 19 }
    ],
    "topCombinations": [
      { "skillA": "Docker",  "skillB": "React",      "offerCount": 14 },
      { "skillA": "CI/CD",   "skillB": "Docker",     "offerCount": 11 },
      { "skillA": "Node.js", "skillB": "TypeScript",  "offerCount": 9  },
      { "skillA": "AWS",     "skillB": "Docker",     "offerCount": 8  },
      { "skillA": "React",   "skillB": "TypeScript",  "offerCount": 7  }
    ]
  },
  "message": null
}
```

---

## Campos

### `topDemand` — Skills más pedidas por las empresas (máx. 10)

Cada ítem representa un skill ordenado de mayor a menor frecuencia en ofertas activas/completadas.

| Campo | Tipo | Descripción |
|---|---|---|
| `skillName` | `string` | Nombre del skill (ej: `"React"`) |
| `categoryName` | `string` | Categoría a la que pertenece (ej: `"FrontEnd"`) |
| `offerCount` | `int` | Número de ofertas distintas que lo incluyen |

---

### `topSupply` — Skills más comunes en candidatos registrados (máx. 10)

Cuántos candidatos distintos declaran tener ese skill en su perfil.

| Campo | Tipo | Descripción |
|---|---|---|
| `skillName` | `string` | Nombre del skill |
| `categoryName` | `string` | Categoría |
| `candidateCount` | `int` | Número de candidatos que lo tienen |

---

### `topCombinations` — Pares de skills que más se piden juntos en ofertas (máx. 10)

Un par aparece aquí cuando la misma oferta incluye ambos skills. Cuanto mayor el `offerCount`,
más frecuente es esa combinación en el mercado.

| Campo | Tipo | Descripción |
|---|---|---|
| `skillA` | `string` | Primer skill del par (orden alfabético) |
| `skillB` | `string` | Segundo skill del par (orden alfabético) |
| `offerCount` | `int` | Ofertas que piden ambos a la vez |

> `skillA` y `skillB` no tienen jerarquía — el orden es solo para evitar duplicados
> (`"React + Docker"` y `"Docker + React"` son el mismo par).

---

## Cuándo usar este endpoint

- Pantalla de bienvenida o landing del candidato antes de completar su perfil.
- Sección "¿Qué busca el mercado?" en la app.
- Página pública de marketing de MatchIQ.
- Dashboard del admin para ver el estado del ecosistema.

No necesita token. Se puede llamar al montar la pantalla sin ningún estado de sesión.

---

---

# 2. Insights personalizados del candidato

## Endpoint

```
GET /api/analytics/market/my-insights
Authorization: Bearer <token>   (rol: Candidate)
```

No recibe parámetros. Devuelve los mismos datos de mercado que el endpoint anterior,
pero cada skill y cada combinación viene anotada con si el candidato autenticado lo tiene
o no. Además incluye dos listas de resumen: fortalezas y brechas.

El candidato debe tener al menos un skill registrado en su perfil para obtener datos útiles.
Si no tiene ningún skill, las listas `skillsInDemand` y `skillGaps` estarán vacías y todas
las banderas `candidateHasSkill` serán `false` — no es un error, es el estado correcto.

---

## Respuesta exitosa

```json
{
  "success": true,
  "data": {
    "topDemand": [
      {
        "skillName": "React",
        "categoryName": "FrontEnd",
        "offerCount": 23,
        "candidateHasSkill": true,
        "candidateLevel": 4
      },
      {
        "skillName": "Docker",
        "categoryName": "DevOps",
        "offerCount": 18,
        "candidateHasSkill": false,
        "candidateLevel": null
      },
      {
        "skillName": "TypeScript",
        "categoryName": "FrontEnd",
        "offerCount": 15,
        "candidateHasSkill": true,
        "candidateLevel": 3
      }
    ],
    "topSupply": [
      { "skillName": "JavaScript", "categoryName": "FrontEnd", "candidateCount": 41 },
      { "skillName": "React",      "categoryName": "FrontEnd", "candidateCount": 37 }
    ],
    "topCombinations": [
      {
        "skillA": "Docker",
        "skillB": "React",
        "offerCount": 14,
        "candidateHasA": false,
        "candidateHasB": true,
        "candidateHasBoth": false
      },
      {
        "skillA": "React",
        "skillB": "TypeScript",
        "offerCount": 7,
        "candidateHasA": true,
        "candidateHasB": true,
        "candidateHasBoth": true
      }
    ],
    "skillsInDemand": ["React", "TypeScript"],
    "skillGaps": ["Docker", "Node.js", "PostgreSQL", "CI/CD", "AWS"]
  },
  "message": null
}
```

---

## Campos adicionales respecto al endpoint público

### `topDemand` — igual que en el endpoint público, más:

| Campo extra | Tipo | Descripción |
|---|---|---|
| `candidateHasSkill` | `bool` | `true` si el candidato tiene ese skill en su perfil |
| `candidateLevel` | `int?` | Nivel de dominio del candidato (1–5). `null` si no lo tiene |

**Escala de niveles:**
| Valor | Significado sugerido |
|---|---|
| `1` | Básico / en aprendizaje |
| `2` | Con práctica limitada |
| `3` | Trabajo independiente |
| `4` | Sólido, proyectos reales |
| `5` | Experto / referente |

---

### `topSupply` — idéntico al endpoint público

Sin campos extra. Sirve para que el candidato vea qué tan competido está el mercado
en los skills que más tiene.

---

### `topCombinations` — igual que en el endpoint público, más:

| Campo extra | Tipo | Descripción |
|---|---|---|
| `candidateHasA` | `bool` | El candidato tiene el skill `skillA` |
| `candidateHasB` | `bool` | El candidato tiene el skill `skillB` |
| `candidateHasBoth` | `bool` | Tiene ambos — cumple esta combinación completa |

**Lógica de UI sugerida para cada combinación:**
- `candidateHasBoth: true` → ✅ "Tienes esta combinación completa"
- `candidateHasA || candidateHasB` (solo uno) → ⚠️ "Tienes X, te falta Y"
- Ninguno → ❌ "No tienes ninguno de estos skills"

---

### `skillsInDemand` — `string[]`

Lista con los nombres de los skills que el candidato tiene Y que aparecen en el `topDemand`
del mercado. Son sus **fortalezas** frente a lo que piden las empresas.

Ejemplo de uso en UI: tarjeta verde "Skills tuyos que el mercado busca".

---

### `skillGaps` — `string[]`

Lista con los nombres de los skills del `topDemand` que el candidato **no tiene**.
Son las **brechas** entre su perfil y lo que demanda el mercado.

El orden importa: el primero es el skill más demandado que le falta. Se puede usar para
mostrar recomendaciones del tipo "Aprende esto primero".

Ejemplo de uso en UI: tarjeta naranja "Skills del mercado que podrías agregar".

---

## Errores posibles

| HTTP | `message` | Causa |
|---|---|---|
| `401` | — | Token ausente, expirado o no es rol Candidate |
| `404` | `"Perfil de candidato no encontrado."` | El usuario Candidate nunca completó ningún dato de perfil |

---

## Flujo de implementación sugerido en Flutter

```
1. Al abrir la pantalla de insights del candidato:
   ├── Mostrar skeleton/loader
   ├── GET /api/analytics/market/my-insights
   │
   ├── Si 404 → redirigir a pantalla "Completa tu perfil primero"
   │
   └── Si 200:
       ├── Sección "Tus fortalezas"   → skillsInDemand (chips verdes)
       ├── Sección "Brechas de mercado" → skillGaps (chips naranjas, primero = más urgente)
       ├── Lista topDemand:
       │   ├── candidateHasSkill=true  → ícono ✅ + mostrar candidateLevel (barra 1-5)
       │   └── candidateHasSkill=false → ícono ⚠️ + CTA "Agregar a mi perfil"
       └── Lista topCombinations:
           ├── candidateHasBoth=true  → ícono ✅
           ├── solo uno               → ícono ⚠️ + "Te falta: [el que no tiene]"
           └── ninguno                → ícono ❌
```

---

## Pantalla pública sin sesión

Para la pantalla informativa antes de que el usuario se registre (landing, onboarding),
usar el endpoint público:

```
GET /api/analytics/market   (sin token)
```

Y mostrar solo `topDemand`, `topSupply` y `topCombinations` sin anotaciones de perfil.
Una vez que el usuario se registra y completa su perfil, cambiar al endpoint de insights
para mostrar la vista personalizada.
