using Microsoft.AspNetCore.Http;

namespace Atracciones.Platform.BuildingBlocks.Middleware;

/// <summary>
/// Genera o reutiliza <c>X-Correlation-ID</c> y lo propaga en la petición y la respuesta.
/// YARP reenvía cabeceras entrantes al cluster; al fijar la cabecera en la petición se garantiza el valor unificado.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var id = context.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(id))
            id = Guid.NewGuid().ToString("D");

        context.Request.Headers[HeaderName] = id;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Remove(HeaderName);
            context.Response.Headers.Append(HeaderName, id);
            return Task.CompletedTask;
        });

        context.Items["CorrelationId"] = id;
        await next(context);
    }
}
