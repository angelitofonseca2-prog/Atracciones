using Atracciones.MsAuditoria.Api.Configuration;
using Atracciones.MsAuditoria.Api.EventBus;
using Atracciones.MsAuditoria.Api.Grpc;
using Atracciones.MsAuditoria.DataAccess;
using Atracciones.MsAuditoria.DataAccess.Context;
using Atracciones.Platform.BuildingBlocks.Kestrel;
using Atracciones.Platform.BuildingBlocks.EventBus.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

DatabaseUrlMapper.Apply("ConnectionStrings__AuditoriaDb");

var builder = WebApplication.CreateBuilder(args);

KestrelGrpcRestPorts.Configure(builder);

var otlp = builder.Configuration["Otlp:Endpoint"];
if (!string.IsNullOrWhiteSpace(otlp))
{
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService(
            serviceName: builder.Configuration["Otlp:ServiceName"] ?? "atracciones-auditoria",
            serviceVersion: "1.0.0"))
        .WithTracing(t => t
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter(o => o.Endpoint = new Uri(otlp)));
}

builder.Services.AddAuditoriaPersistence(builder.Configuration);
builder.Services.AddAtraccionesEventBus(builder.Configuration, services =>
{
    services.AddHostedService<MarketplaceAuditoriaConsumerHostedService>();
});
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<AuditoriaGrpcService>();
app.MapGet("/health", () => Results.Json(new { status = "ok" }));

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AuditoriaDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AuditoriaDb");
            await db.Database.MigrateAsync();
            logger.LogInformation("Migraciones del esquema audit aplicadas.");
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AuditoriaDb");
            logger.LogError(ex,
                "No se pudieron aplicar migraciones de auditoría. Revise Postgres (:5440) y ConnectionStrings:AuditoriaDb.");
        }
    });
});

app.Run();
