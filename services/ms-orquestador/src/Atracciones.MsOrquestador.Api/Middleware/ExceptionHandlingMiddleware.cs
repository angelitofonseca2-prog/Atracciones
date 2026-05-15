using System.Text.Json;
using Atracciones.MsOrquestador.Api.Models.Common;
using Atracciones.MsOrquestador.Business.Exceptions;
using Grpc.Core;

namespace Atracciones.MsOrquestador.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next) => _next = next;

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

    private static async Task HandleAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        ApiErrorResponse body;
        switch (ex)
        {
            case JsonException:
                context.Response.StatusCode = 400;
                body = new ApiErrorResponse { Status = 400, Message = "JSON inválido", Details = new List<string> { ex.Message }, Path = context.Request.Path.ToString() };
                break;
            case ValidationOrchestadorException ve:
                context.Response.StatusCode = 400;
                body = new ApiErrorResponse { Status = 400, Message = "Parámetro inválido", Details = ve.Errores.ToList(), Path = context.Request.Path.ToString() };
                break;
            case NotFoundOrchestadorException nf:
                context.Response.StatusCode = 404;
                body = new ApiErrorResponse { Status = 404, Message = "Recurso no encontrado", Details = new List<string> { nf.Message }, Path = context.Request.Path.ToString() };
                break;
            case ConflictOrchestadorException cf:
                context.Response.StatusCode = 409;
                body = new ApiErrorResponse { Status = 409, Message = "Conflicto", Details = new List<string> { cf.Message }, Path = context.Request.Path.ToString() };
                break;
            case ForbiddenOrchestadorException fb:
                context.Response.StatusCode = 403;
                body = new ApiErrorResponse { Status = 403, Message = "Prohibido", Details = new List<string> { fb.Message }, Path = context.Request.Path.ToString() };
                break;
            case UnauthorizedAccessException ua:
                context.Response.StatusCode = 401;
                body = new ApiErrorResponse { Status = 401, Message = "No autorizado", Details = new List<string> { ua.Message }, Path = context.Request.Path.ToString() };
                break;
            case ServiceUnavailableOrchestadorException su:
                context.Response.StatusCode = 503;
                body = new ApiErrorResponse { Status = 503, Message = "Servicio no disponible", Details = new List<string> { su.Message }, Path = context.Request.Path.ToString() };
                break;
            case RpcException rpc:
                context.Response.StatusCode = MapGrpc(rpc.StatusCode);
                body = new ApiErrorResponse
                {
                    Status = context.Response.StatusCode,
                    Message = "Error de dependencia gRPC",
                    Details = new List<string> { rpc.Status.Detail },
                    Path = context.Request.Path.ToString(),
                };
                break;
            default:
                context.Response.StatusCode = 500;
                body = new ApiErrorResponse { Status = 500, Message = "Error interno", Details = new List<string> { "Ocurrió un error inesperado." }, Path = context.Request.Path.ToString() };
                break;
        }

        var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, opts));
    }

    private static int MapGrpc(StatusCode code) => code switch
    {
        StatusCode.InvalidArgument => 400,
        StatusCode.NotFound => 404,
        StatusCode.AlreadyExists => 409,
        StatusCode.FailedPrecondition => 409,
        StatusCode.PermissionDenied => 403,
        StatusCode.Unauthenticated => 401,
        _ => 503,
    };
}
