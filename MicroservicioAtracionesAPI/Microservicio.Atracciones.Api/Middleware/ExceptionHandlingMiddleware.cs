using System.Text.Json;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.Exceptions;

namespace Microservicio.Atracciones.Api.Middleware;

/// <summary>
/// Middleware global de manejo de excepciones.
/// Captura cualquier excepción no controlada y la convierte en la
/// estructura de error uniforme del contrato (sección 6).
/// 
/// Mapeo de excepciones → HTTP:
///   ValidationException             → 400
///   NotFoundException               → 404
///   UnauthorizedBusinessException   → 401
///   ForbiddenBusinessException      → 403
///   ConflictException               → 409
///   BusinessException               → código embebido en la excepción
///   Exception (no controlada)       → 500
/// </summary>
public class ExceptionHandlingMiddleware
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
            await ManejarExcepcionAsync(context, ex);
        }
    }

    private async Task ManejarExcepcionAsync(HttpContext context, Exception ex)
    {
        var path = context.Request.Path.ToString();
        var response = context.Response;
        response.ContentType = "application/json";

        ApiErrorResponse errorResponse;

        switch (ex)
        {
            case ValidationException validationEx:
                _logger.LogWarning("Validación fallida en {Path}: {Details}", path, validationEx.Details);
                response.StatusCode = 400;
                errorResponse = new ApiErrorResponse
                {
                    Status = 400,
                    Error = "Parámetro inválido",
                    Details = validationEx.Details.ToList(),
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Path = path
                };
                break;

            case NotFoundException notFoundEx:
                _logger.LogInformation("Recurso no encontrado en {Path}: {Message}", path, notFoundEx.Message);
                response.StatusCode = 404;
                errorResponse = new ApiErrorResponse
                {
                    Status = 404,
                    Error = "Recurso no encontrado",
                    Details = new List<string> { notFoundEx.Message },
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Path = path
                };
                break;

            case UnauthorizedBusinessException unauthorizedEx:
                _logger.LogWarning("Acceso no autorizado en {Path}: {Message}", path, unauthorizedEx.Message);
                response.StatusCode = 401;
                errorResponse = new ApiErrorResponse
                {
                    Status = 401,
                    Error = "No autorizado",
                    Details = new List<string> { unauthorizedEx.Message },
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Path = path
                };
                break;

            case ForbiddenBusinessException forbiddenEx:
                _logger.LogWarning("Acceso prohibido en {Path}: {Message}", path, forbiddenEx.Message);
                response.StatusCode = 403;
                errorResponse = new ApiErrorResponse
                {
                    Status = 403,
                    Error = "Acceso prohibido",
                    Details = new List<string> { forbiddenEx.Message },
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Path = path
                };
                break;

            case ConflictException conflictEx:
                _logger.LogWarning("Conflicto de negocio en {Path}: {Message}", path, conflictEx.Message);
                response.StatusCode = 409;
                errorResponse = new ApiErrorResponse
                {
                    Status = 409,
                    Error = "Conflicto",
                    Details = new List<string> { conflictEx.Message },
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Path = path
                };
                break;

            case BusinessException businessEx:
                _logger.LogWarning("Error de negocio en {Path}: {Message}", path, businessEx.Message);
                response.StatusCode = businessEx.HttpStatusCode;
                errorResponse = new ApiErrorResponse
                {
                    Status = businessEx.HttpStatusCode,
                    Error = businessEx.Message,
                    Details = new List<string> { businessEx.Message },
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Path = path
                };
                break;

            default:
                _logger.LogError(ex, "Error no controlado en {Path}", path);
                response.StatusCode = 500;
                errorResponse = new ApiErrorResponse
                {
                    Status = 500,
                    Error = "Error interno del servidor",
                    Details = new List<string> { "Ocurrió un error inesperado. Intente nuevamente." },
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Path = path
                };
                break;
        }

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }; // #6: snake_case consistente
        var json = JsonSerializer.Serialize(errorResponse, options);
        await response.WriteAsync(json);
    }
}
