using System.Text.Json;
using Atracciones.MsReservas.Api.Models.Common;

namespace Atracciones.MsReservas.Api.Middleware;

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
            case ArgumentException ae:
                context.Response.StatusCode = 400;
                body = new ApiErrorResponse { Status = 400, Error = "Solicitud inválida", Details = new List<string> { ae.Message }, Path = context.Request.Path.ToString() };
                break;
            case InvalidOperationException ie:
                context.Response.StatusCode = 409;
                body = new ApiErrorResponse { Status = 409, Error = "Conflicto", Details = new List<string> { ie.Message }, Path = context.Request.Path.ToString() };
                break;
            case KeyNotFoundException knf:
                context.Response.StatusCode = 404;
                body = new ApiErrorResponse { Status = 404, Error = "No encontrado", Details = new List<string> { knf.Message }, Path = context.Request.Path.ToString() };
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
