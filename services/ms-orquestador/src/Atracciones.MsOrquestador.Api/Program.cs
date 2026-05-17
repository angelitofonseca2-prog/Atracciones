using System.Text.Json;
using Atracciones.MsOrquestador.Api.Configuration;
using Atracciones.MsOrquestador.Api.Extensions;
using Atracciones.MsOrquestador.Api.Middleware;
using Atracciones.MsOrquestador.Business;
using Atracciones.MsOrquestador.Business.Options;
using Atracciones.MsOrquestador.DataAccess;
using Atracciones.MsOrquestador.DataAccess.Context;
using Microsoft.Extensions.Options;
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

app.Lifetime.ApplicationStarted.Register(() =>
{
    var grpcLog = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("GrpcClients");
    var grpc = app.Services.GetRequiredService<IOptions<GrpcClientsOptions>>().Value;
    grpcLog.LogInformation(
        "Destinos gRPC: Identidad={Identidad}, Clientes={Clientes}, Reservas={Reservas}, Atracciones={Atracciones}",
        grpc.Identidad, grpc.Clientes, grpc.Reservas, grpc.Atracciones);

    _ = Task.Run(async () =>
    {
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<OrquestadorDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("OrquestadorDb");
            await db.Database.MigrateAsync();
            logger.LogInformation("Migraciones del esquema orquestador aplicadas.");
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("OrquestadorDb");
            logger.LogError(ex,
                "No se pudieron aplicar migraciones de orquestador. Revise Postgres (:5438) y ConnectionStrings:OrquestadorDb.");
        }
    });
});

app.Run();
