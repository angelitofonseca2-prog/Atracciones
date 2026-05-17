using System.Text.Json;
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

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

            var inventarioDb = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
            var invLogger = loggerFactory.CreateLogger("InventarioDb");
            await inventarioDb.Database.MigrateAsync();
            invLogger.LogInformation("Migraciones del esquema inventario aplicadas.");

            var catalogosDb = scope.ServiceProvider.GetRequiredService<CatalogosDbContext>();
            var catLogger = loggerFactory.CreateLogger("CatalogosDb");
            await catalogosDb.Database.MigrateAsync();
            catLogger.LogInformation("Migraciones del esquema catalogos aplicadas.");
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("InventarioDb");
            logger.LogError(ex,
                "No se pudieron aplicar migraciones. Revise Postgres (:5436) y ConnectionStrings:InventarioDb/CatalogosDb.");
        }
    });
});

app.Run();
