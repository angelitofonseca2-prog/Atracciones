using System.Text.Json;
using Atracciones.MsFacturacion.Api.Configuration;
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

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("BillingDb");
            await db.Database.MigrateAsync();
            logger.LogInformation("Migraciones del esquema billing aplicadas.");
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("BillingDb");
            logger.LogError(ex,
                "No se pudieron aplicar migraciones de billing. Revise Postgres (:5439) y ConnectionStrings:BillingDb.");
        }
    });
});

app.Run();
