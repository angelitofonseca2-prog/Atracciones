using System.Text.Json;
using Atracciones.BuildingBlocks.Database;
using Atracciones.MsOrquestador.Api.Configuration;
using Atracciones.MsOrquestador.Api.Extensions;
using Atracciones.MsOrquestador.Api.Middleware;
using Atracciones.MsOrquestador.Business;
using Atracciones.MsOrquestador.DataAccess;
using Atracciones.MsOrquestador.DataAccess.Context;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

DatabaseUrlMapper.Apply("ConnectionStrings__OrquestadorDb");

// Permite gRPC sobre HTTP/2 sin TLS (h2c) para Railway Private Networking.
// Los canales gRPC apuntan a http://servicio.railway.internal:8080
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

var otlp = builder.Configuration["Otlp:Endpoint"];
if (!string.IsNullOrWhiteSpace(otlp))
{
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService(
            serviceName: builder.Configuration["Otlp:ServiceName"] ?? "atracciones-orquestador",
            serviceVersion: "1.0.0"))
        .WithTracing(t => t
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter(o => o.Endpoint = new Uri(otlp)));
}

builder.WebHost.ConfigureKestrel(o =>
    o.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http1AndHttp2));

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

builder.Services.AddOrquestadorPersistence(builder.Configuration);
builder.Services.AddOrquestadorBusiness(builder.Configuration);
builder.Services.AddJwtFromJwks(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.Use(async (ctx, next) =>
{
    ctx.Request.EnableBuffering();
    await next();
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Json(new { status = "ok" }));

{
    var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("OrquestadorDb");
    try
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrquestadorDbContext>();
        await EfMigrationHistoryBaseline.MigrateWithBaselineAsync(
            db, "orq", "orq", "saga_state",
            ["20260510205821_InitialOrquestador"], startupLogger);
        startupLogger.LogInformation("Migraciones del esquema orquestador aplicadas.");
    }
    catch (Exception ex)
    {
        startupLogger.LogCritical(ex,
            "No se pudieron aplicar migraciones de orquestador. Revise DATABASE_URL en Railway.");
        throw;
    }
}

app.Run();
