namespace MatchIQ.API.Middlewares;

// Middleware que extrae el userId y role del JWT y los expone como servicio
// Los servicios de Application lo inyectan para saber qué usuario está activo
// Equivalente a leer req.user en el middleware de auth de Express
public class CurrentUserMiddleware
{
    // TODO: constructor con RequestDelegate next
    // TODO: InvokeAsync(HttpContext context)
    //       lee los claims del JWT (sub → userId, role → UserRole)
    //       los guarda en ICurrentUserService para que lo inyecten los services
}
