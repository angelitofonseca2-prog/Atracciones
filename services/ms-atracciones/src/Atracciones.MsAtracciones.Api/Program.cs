using System.Text.Json;
using Atracciones.BuildingBlocks.Database;
using Atracciones.MsAtracciones.Api.Configuration;
using Atracciones.MsAtracciones.Api.Extensions;
using Atracciones.MsAtracciones.Api.Grpc;
using Atracciones.MsAtracciones.Api.Middleware;
using Atracciones.MsAtracciones.Business;
using Atracciones.MsAtracciones.DataAccess;
using Atracciones.MsAtracciones.DataAccess.Context;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

DatabaseUrlMapper.Apply("ConnectionStrings__InventarioDb");
DatabaseUrlMapper.Apply("ConnectionStrings__CatalogosDb");

var builder = WebApplication.CreateBuilder(args);

var inventarioCs = builder.Configuration.GetConnectionString("InventarioDb");
if (string.IsNullOrWhiteSpace(inventarioCs))
{
    throw new InvalidOperationException(
        "Falta ConnectionStrings:InventarioDb. Use `dotnet run --launch-profile http` o ConnectionStrings__InventarioDb.");
}

builder.WebHost.ConfigureKestrel(o =>
    o.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http1AndHttp2));

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

builder.Services.AddInventarioPersistence(builder.Configuration);
builder.Services.AddInventarioBusiness(builder.Configuration);
builder.Services.AddJwtFromJwks(builder.Configuration);
builder.Services.AddGrpc();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<InventarioGrpcService>();
app.MapGrpcService<CatalogGrpcService>();
app.MapGet("/health", () => Results.Json(new { status = "ok" }));

{
    var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AtraccionesDb");
    try
    {
        await using var scope = app.Services.CreateAsyncScope();
        var inventarioDb = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
        await EfMigrationHistoryBaseline.MigrateWithBaselineAsync(
            inventarioDb, "inventario", "inventario", "atracciones",
            ["20260510203501_InitialInventario"], startupLogger);
        startupLogger.LogInformation("Migraciones del esquema inventario aplicadas.");

        var catalogosDb = scope.ServiceProvider.GetRequiredService<CatalogosDbContext>();
        await EfMigrationHistoryBaseline.MigrateWithBaselineAsync(
            catalogosDb, "catalogos", "catalogos", "categorias",
            ["20260513224040_InitialCatalogos"], startupLogger);
        startupLogger.LogInformation("Migraciones del esquema catalogos aplicadas.");
    }
    catch (Exception ex)
    {
        startupLogger.LogCritical(ex,
            "No se pudieron aplicar migraciones inventario/catalogos. Revise DATABASE_URL en Railway.");
        throw;
    }
}

app.Run();
