using Microsoft.AspNetCore.Builder;

namespace Atracciones.Platform.BuildingBlocks.Idempotency;

/// <summary>
/// Fase 0: solo documenta el header; la validación obligatoria llegará con ms-orquestador.
/// Opcionalmente se puede activar logging cuando falte en rutas concretas.
/// </summary>
public static class IdempotencyKeyExtensions
{
    public static IApplicationBuilder UseIdempotencyKeyProbe(this IApplicationBuilder app)
        => app.Use(async (ctx, next) =>
        {
            // Reservado: en fases posteriores validar Idempotency-Key en POST de orquestador.
            await next();
        });
}
