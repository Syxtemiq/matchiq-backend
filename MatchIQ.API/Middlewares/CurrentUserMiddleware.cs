// Este middleware no es necesario con el patrón IHttpContextAccessor.
// La extracción de claims del JWT la hace CurrentUserService directamente.
// El archivo se conserva por si se requiere lógica adicional por request en el futuro.

namespace MatchIQ.API.Middlewares;

public class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context) => _next(context);
}
