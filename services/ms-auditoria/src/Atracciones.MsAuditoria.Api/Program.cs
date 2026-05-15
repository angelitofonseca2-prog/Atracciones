using Atracciones.MsAuditoria.Api.Grpc;
using Atracciones.MsAuditoria.DataAccess;
using Atracciones.MsAuditoria.DataAccess.Context;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var grpcPort = builder.Configuration.GetValue<int?>("GrpcPort");
if (grpcPort.HasValue)
{
    builder.WebHost.ConfigureKestrel(kestrel =>
    {
        kestrel.ListenAnyIP(8080, o => o.Protocols = HttpProtocols.Http1);
        kestrel.ListenAnyIP(grpcPort.Value, o => o.Protocols = HttpProtocols.Http2);
    });
}
else
{
    builder.WebHost.ConfigureKestrel(o =>
        o.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http1AndHttp2));
}

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
