using System.Text.Json;
using Atracciones.BuildingBlocks.Database;
using Atracciones.MsReservas.Api.Configuration;
using Atracciones.MsReservas.Api.Extensions;
using Atracciones.MsReservas.Api.Grpc;
using Atracciones.MsReservas.Api.Middleware;
using Atracciones.MsReservas.Api.Options;
using Atracciones.MsReservas.DataAccess;
using Atracciones.MsReservas.DataAccess.Context;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

DatabaseUrlMapper.Apply("ConnectionStrings__VentasDb");
DatabaseUrlMapper.Apply("ConnectionStrings__CrmDb");

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o =>
    o.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http1AndHttp2));

builder.Services.Configure<ClientesMirrorOptions>(builder.Configuration.GetSection(ClientesMirrorOptions.SectionName));

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

builder.Services.AddVentasPersistence(builder.Configuration);
builder.Services.AddJwtFromJwks(builder.Configuration);
builder.Services.AddGrpc();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<ReservaGrpcService>();
app.MapGrpcService<ClienteGrpcService>();
app.MapGet("/health", () => Results.Json(new { status = "ok" }));

{
    var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("ReservasDb");
    try
    {
        await using var scope = app.Services.CreateAsyncScope();
        var ventasDb = scope.ServiceProvider.GetRequiredService<VentasDbContext>();
        await EfMigrationHistoryBaseline.MigrateWithBaselineAsync(
            ventasDb, "ventas", "ventas", "reservas",
            ["20260510204836_InitialVentas"], startupLogger);
        startupLogger.LogInformation("Migraciones del esquema ventas aplicadas.");

        var crmDb = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        await EfMigrationHistoryBaseline.MigrateWithBaselineAsync(
            crmDb, "crm", "crm", "clientes",
            ["20260513224038_InitialCrm"], startupLogger);
        startupLogger.LogInformation("Migraciones del esquema crm aplicadas.");
    }
    catch (Exception ex)
    {
        startupLogger.LogCritical(ex,
            "No se pudieron aplicar migraciones ventas/crm. Revise DATABASE_URL en Railway.");
        throw;
    }
}

app.MapGet("/health/db", async (CrmDbContext crm, VentasDbContext ventas, CancellationToken ct) =>
{
    try
    {
        _ = await crm.Clientes.AsNoTracking().AnyAsync(ct);
        _ = await ventas.Reservas.AsNoTracking().AnyAsync(ct);
        return Results.Json(new { status = "ok", schemas = new[] { "crm", "ventas" } });
    }
    catch (Exception ex)
    {
        return Results.Json(new { status = "error", message = ex.Message }, statusCode: 503);
    }
});

app.Run();
