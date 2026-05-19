using System.Text.Json;
using Atracciones.Contracts.Inventario.V1;
using Atracciones.MsReservas.Api.Configuration;
using Atracciones.MsReservas.Api.Extensions;
using Atracciones.MsReservas.Api.Grpc;
using Atracciones.MsReservas.Api.Integration;
using Atracciones.MsReservas.Api.Middleware;
using Atracciones.MsReservas.Api.Options;
using Atracciones.MsReservas.Api.Services;
using Atracciones.MsReservas.DataAccess;
using Atracciones.MsReservas.DataAccess.Context;
using Atracciones.Platform.BuildingBlocks.Kestrel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

DatabaseUrlMapper.Apply("ConnectionStrings__VentasDb");
DatabaseUrlMapper.Apply("ConnectionStrings__CrmDb");

var builder = WebApplication.CreateBuilder(args);

KestrelGrpcRestPorts.Configure(builder);

builder.Services.Configure<ClientesMirrorOptions>(builder.Configuration.GetSection(ClientesMirrorOptions.SectionName));

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

builder.Services.AddVentasPersistence(builder.Configuration);
builder.Services.AddJwtFromJwks(builder.Configuration);
builder.Services.AddOptions<GrpcClientsOptions>()
    .BindConfiguration(GrpcClientsOptions.SectionName)
    .PostConfigure(o => o.Atracciones = GrpcBaseUrlNormalizer.NormalizeGrpc(o.Atracciones));
builder.Services.AddSingleton<InventarioGrpcChannelHolder>();
builder.Services.AddSingleton(sp =>
    new AtraccionInventarioService.AtraccionInventarioServiceClient(
        sp.GetRequiredService<InventarioGrpcChannelHolder>().Atracciones));
builder.Services.AddScoped<ReservaAdminAppService>();
builder.Services.AddGrpc();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<ReservaGrpcService>();
app.MapGrpcService<ClienteGrpcService>();
app.MapGet("/health", () => Results.Json(new { status = "ok" }));

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        try
        {
            await using var scope = app.Services.CreateAsyncScope();

            var ventasDb = scope.ServiceProvider.GetRequiredService<VentasDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("VentasDb");
            await ventasDb.Database.MigrateAsync();
            logger.LogInformation("Migraciones del esquema ventas aplicadas.");

            var crmDb = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
            await crmDb.Database.MigrateAsync();
            logger.LogInformation("Migraciones del esquema crm aplicadas.");
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("VentasDb");
            logger.LogError(ex,
                "No se pudieron aplicar migraciones. Revise Postgres (:5437) y ConnectionStrings:VentasDb/CrmDb.");
        }
    });
});

app.Run();
