using System.Net;
using System.Text.Json;
using MatchIQ.API.Common;

namespace MatchIQ.API.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción no controlada: {Message}", ex.Message);
            await WriteErrorAsync(context, ex);
        }
    }

    private static Task WriteErrorAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            InvalidOperationException   => (HttpStatusCode.BadRequest,        ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized,       ex.Message),
            KeyNotFoundException        => (HttpStatusCode.NotFound,           ex.Message),
            NotImplementedException     => (HttpStatusCode.NotImplemented,     "Funcionalidad no implementada."),
            _                          => (HttpStatusCode.InternalServerError, "Ocurrió un error interno.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var body = JsonSerializer.Serialize(
            ApiResponse.Fail(message),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        return context.Response.WriteAsync(body);
    }
}
