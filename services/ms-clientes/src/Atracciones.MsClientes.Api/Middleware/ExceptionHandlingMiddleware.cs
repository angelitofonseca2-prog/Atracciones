using System.Text.Json;
using Atracciones.MsClientes.Api.Models;
using Atracciones.MsClientes.Business.Exceptions;

namespace Atracciones.MsClientes.Api.Middleware;

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
            case ValidationException ve:
                context.Response.StatusCode = 400;
                body = new ApiErrorResponse { Status = 400, Error = "Parámetro inválido", Details = ve.Errores.ToList(), Path = context.Request.Path.ToString() };
                break;
            case UnauthorizedBusinessException ue:
                context.Response.StatusCode = 401;
                body = new ApiErrorResponse { Status = 401, Error = "No autorizado", Details = new List<string> { ue.Message }, Path = context.Request.Path.ToString() };
                break;
            case NotFoundException ne:
                context.Response.StatusCode = 404;
                body = new ApiErrorResponse { Status = 404, Error = "Recurso no encontrado", Details = new List<string> { ne.Message }, Path = context.Request.Path.ToString() };
                break;
            default:
                context.Response.StatusCode = 500;
                body = new ApiErrorResponse { Status = 500, Error = "Error interno", Details = new List<string> { "Ocurrió un error inesperado." }, Path = context.Request.Path.ToString() };
                break;
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(body,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }));
    }
}
