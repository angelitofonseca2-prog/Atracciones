using System.Text.Json;
using Atracciones.MsCatalogos.Api.Extensions;
using Atracciones.MsCatalogos.Api.Grpc;
using Atracciones.MsCatalogos.Api.Integration;
using Atracciones.MsCatalogos.Api.Middleware;
using Atracciones.MsCatalogos.Business;
using Atracciones.MsCatalogos.Business.Integration;
using Atracciones.MsCatalogos.DataAccess;
using Atracciones.MsCatalogos.DataAccess.Context;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Si falta cadena de conexión (p. ej. `dotnet run` sin perfil Development), fallar antes con mensaje claro.
var catalogCs = builder.Configuration.GetConnectionString("CatalogosDb");
if (string.IsNullOrWhiteSpace(catalogCs))
{
    throw new InvalidOperationException(
        "Falta ConnectionStrings:CatalogosDb. Use `dotnet run --launch-profile http` o defina la variable de entorno ConnectionStrings__CatalogosDb.");
}

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

builder.Services.AddCatalogosPersistence(builder.Configuration);
builder.Services.AddCatalogosBusiness(builder.Configuration);

builder.Services.AddHttpClient<IMonolithCatalogLegacyPublisher, MonolithCatalogLegacyPublisher>((sp, client) =>
{
    var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MonolithCatalogLegacySyncOptions>>().Value;
    if (!string.IsNullOrWhiteSpace(opts.BaseUrl))
        client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(20);
});

builder.Services.AddJwtFromJwks(builder.Configuration);
builder.Services.AddGrpc();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<CatalogGrpcService>();
app.MapGet("/health", () => Results.Json(new { status = "ok" }));

// Migraciones después de que el host escuche: así el puerto (p. ej. 5301) queda abierto y el navegador no ve "connection refused"
// si Postgres aún no está listo; los endpoints que usan BD fallarán hasta que la migración termine bien.
app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<CatalogosDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("CatalogosDb");
            await db.Database.MigrateAsync();
            logger.LogInformation("Migraciones del esquema catalogos aplicadas.");
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("CatalogosDb");
            logger.LogError(ex,
                "No se pudieron aplicar migraciones de catalogos. Revise Postgres (local :5435 o docker compose postgres-catalog) y ConnectionStrings:CatalogosDb.");
        }
    });
});

app.Run();
