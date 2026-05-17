using Atracciones.BuildingBlocks.Database;
using Atracciones.MsAuditoria.Api.Grpc;
using Atracciones.MsAuditoria.DataAccess;
using Atracciones.MsAuditoria.DataAccess.Context;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

DatabaseUrlMapper.Apply("ConnectionStrings__AuditoriaDb");

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o =>
    o.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http1AndHttp2));

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
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<AuditoriaGrpcService>();
app.MapGet("/health", () => Results.Json(new { status = "ok" }));

{
    var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AuditoriaDb");
    try
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditoriaDbContext>();
        await EfMigrationHistoryBaseline.MigrateWithBaselineAsync(
            db, "audit", "audit", "eventos",
            ["20260510214720_InitialAuditoria"], startupLogger);
        startupLogger.LogInformation("Migraciones del esquema audit aplicadas.");
    }
    catch (Exception ex)
    {
        startupLogger.LogCritical(ex,
            "No se pudieron aplicar migraciones de auditoría. Revise DATABASE_URL en Railway.");
        throw;
    }
}

app.Run();
