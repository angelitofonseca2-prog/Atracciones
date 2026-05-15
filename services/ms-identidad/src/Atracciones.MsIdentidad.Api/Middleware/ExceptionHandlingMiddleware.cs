using System.Text.Json;
using Atracciones.MsIdentidad.Api.Models;
using Atracciones.MsIdentidad.Business.Exceptions;

namespace Atracciones.MsIdentidad.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var path = context.Request.Path.ToString();
        context.Response.ContentType = "application/json";

        ApiErrorResponse body;
        switch (ex)
        {
            case ValidationException ve:
                context.Response.StatusCode = 400;
                body = new ApiErrorResponse
                {
                    Status = 400,
                    Error = "Parámetro inválido",
                    Details = ve.Errores.ToList(),
                    Path = path,
                };
                break;
            case UnauthorizedBusinessException ue:
                context.Response.StatusCode = 401;
                body = new ApiErrorResponse
                {
                    Status = 401,
                    Error = "No autorizado",
                    Details = new List<string> { ue.Message },
                    Path = path,
                };
                break;
            case InvalidOperationException ioe when ioe.Message.Contains("Login", StringComparison.Ordinal)
                || ioe.Message.Contains("Roles no encontrados", StringComparison.Ordinal):
                context.Response.StatusCode = 409;
                body = new ApiErrorResponse
                {
                    Status = 409,
                    Error = "Conflicto",
                    Details = new List<string> { ex.Message },
                    Path = path,
                };
                break;
            default:
                _logger.LogError(ex, "Error no controlado en {Path}", path);
                context.Response.StatusCode = 500;
                body = new ApiErrorResponse
                {
                    Status = 500,
                    Error = "Error interno",
                    Details = new List<string> { "Ocurrió un error inesperado." },
                    Path = path,
                };
                break;
        }

        var json = JsonSerializer.Serialize(
            body,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
        await context.Response.WriteAsync(json);
    }
}
