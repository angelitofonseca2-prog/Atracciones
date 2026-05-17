using System.Text.Json;
using Atracciones.BuildingBlocks.Database;
using Atracciones.MsFacturacion.Api.Extensions;
using Atracciones.MsFacturacion.Api.Grpc;
using Atracciones.MsFacturacion.Api.Middleware;
using Atracciones.MsFacturacion.DataAccess;
using Atracciones.MsFacturacion.DataAccess.Context;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

DatabaseUrlMapper.Apply("ConnectionStrings__BillingDb");

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o =>
    o.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http1AndHttp2));

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

builder.Services.AddBillingPersistence(builder.Configuration);
builder.Services.AddJwtFromJwks(builder.Configuration);
builder.Services.AddGrpc();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<FacturaGrpcService>();
app.MapGet("/health", () => Results.Json(new { status = "ok" }));

{
    var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("BillingDb");
    try
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        await EfMigrationHistoryBaseline.MigrateWithBaselineAsync(
            db, "billing", "billing", "facturas",
            ["20260510213512_InitialBilling"], startupLogger);
        startupLogger.LogInformation("Migraciones del esquema billing aplicadas.");
    }
    catch (Exception ex)
    {
        startupLogger.LogCritical(ex,
            "No se pudieron aplicar migraciones de billing. Revise DATABASE_URL en Railway.");
        throw;
    }
}

app.Run();
