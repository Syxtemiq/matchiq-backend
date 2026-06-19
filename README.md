# MatchIQ Backend — .NET 9

Plataforma de matching entre empresas y candidatos desarrolladores, con evaluación por IA.

## Estructura de la solución

```
MatchIQ.Domain          → Entidades, enums e interfaces (sin dependencias externas)
MatchIQ.Application     → Lógica de negocio y servicios por módulo
MatchIQ.Infrastructure  → EF Core, OpenAI, MailKit (implementaciones concretas)
MatchIQ.API             → Controllers, middlewares, Program.cs
```

## Regla de dependencias

```
Domain ← Application ← Infrastructure
                     ← API → Application
```

## Requisitos

- .NET 9 SDK
- PostgreSQL 15+
- Cuenta OpenAI (API key)
- Cuenta Google Cloud (OAuth 2.0 para Flutter Web)

## Configuración inicial

1. Clonar el repo
2. Copiar `appsettings.json` y completar las variables (conexión DB, API keys)
3. Instalar NuGet packages (ver comentarios en cada .csproj)
4. Crear la base de datos y correr migraciones:
   ```
   dotnet ef migrations add InitialCreate --project MatchIQ.Infrastructure --startup-project MatchIQ.API
   dotnet ef database update --project MatchIQ.Infrastructure --startup-project MatchIQ.API
   ```
5. Correr el servidor:
   ```
   dotnet run --project MatchIQ.API
   ```

## NuGet packages necesarios

### MatchIQ.Infrastructure
```
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Betalgo.OpenAI
dotnet add package MailKit
```

### MatchIQ.API
```
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.AspNetCore.Authentication.Google
dotnet add package Swashbuckle.AspNetCore
dotnet add package Microsoft.EntityFrameworkCore.Design
```

## Módulos principales

| Módulo | Descripción |
|---|---|
| Auth | Registro, login, verificación email, Google OAuth, JWT |
| Candidate | Perfil del candidato, categorías y skills |
| Company | Perfil de la empresa |
| Offers | CRUD de ofertas + parser de IA (feature nueva) |
| Matching | Algoritmo SQL + insight IA para top 3 |
| Tests | Generación por IA, editor con chat, submissions |
| Admin | Gestión de usuarios del sistema |
| Catalog | Categorías y skills disponibles |

## Feature nueva: Offer Parser con IA

`POST /api/offers/parse-description`

Recibe una descripción libre en texto y la IA extrae los campos estructurados
para pre-llenar el formulario de creación de oferta en el frontend Flutter.

## Chat de edición de preguntas

Cada pregunta del test tiene su propio historial de chat.
El admin de la empresa puede pedirle a la IA que modifique una pregunta específica
y la IA regenera solo esa pregunta manteniendo el contexto de la conversación.

`POST /api/tests/questions/{questionId}/chat`
