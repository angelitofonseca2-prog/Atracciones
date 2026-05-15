using Microsoft.AspNetCore.Builder;

namespace Atracciones.Platform.BuildingBlocks.Middleware;

public static class CorrelationIdExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}
