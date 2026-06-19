namespace MatchIQ.API.Middlewares;

// Middleware global de manejo de errores
// Captura excepciones no controladas y retorna respuestas JSON consistentes
// Equivalente a error.middleware.js del backend Node
// Mapea tipos de excepción a códigos HTTP:
//   - InvalidOperationException → 400
//   - UnauthorizedAccessException → 401
//   - KeyNotFoundException → 404
//   - Exception genérica → 500
public class ErrorHandlingMiddleware
{
    // TODO: constructor con RequestDelegate next, ILogger
    // TODO: InvokeAsync(HttpContext context)
    //       try/catch que atrapa todo y retorna { error: message } en JSON
}
