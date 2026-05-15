using System.Text.Json;
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
