using Atracciones.BuildingBlocks.Database;
using Atracciones.MsIdentidad.Api.Configuration;
using Atracciones.MsIdentidad.Api.Extensions;
using Atracciones.MsIdentidad.Api.Grpc;
using Atracciones.MsIdentidad.Api.Middleware;
using Atracciones.MsIdentidad.Api.Options;
using Atracciones.MsIdentidad.Api.Security;
using Atracciones.MsIdentidad.Api.Services;
using Atracciones.MsIdentidad.Business;
using Atracciones.MsIdentidad.Business.Auth;
using Atracciones.MsIdentidad.DataAccess;
using Atracciones.MsIdentidad.DataAccess.Context;
using Atracciones.MsIdentidad.DataAccess.Seeding;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

DatabaseUrlMapper.Apply("ConnectionStrings__IdentidadDb");

var builder = WebApplication.CreateBuilder(args);

// REST y gRPC en el mismo listener (HTTP/1 + HTTP/2). Evita HTTP_1_1_REQUIRED si gRPC usa el puerto 8080 (Railway/orquestador).
builder.WebHost.ConfigureKestrel(o =>
    o.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http1AndHttp2));

builder.Services.Configure<JwtIssuerOptions>(builder.Configuration.GetSection(JwtIssuerOptions.SectionName));
builder.Services.Configure<InternalSyncOptions>(builder.Configuration.GetSection(InternalSyncOptions.SectionName));

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
    });

builder.Services.AddIdentidadJwtBearer();
builder.Services.AddIdentidadPersistence(builder.Configuration);
builder.Services.AddIdentidadBusiness();
builder.Services.AddSingleton(sp =>
    IdentidadSigningKeysFactory.Create(
        sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<JwtIssuerOptions>>(),
        sp.GetRequiredService<IHostEnvironment>(),
        sp.GetRequiredService<ILoggerFactory>()));
builder.Services.AddSingleton<IJwtTokenIssuer, RsaJwtTokenIssuer>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddGrpc();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<UsuarioGrpcService>();
app.MapGet("/.well-known/jwks.json", (IdentidadSigningKeys keys) =>
    Results.Text(keys.BuildJwksJson(), "application/json"));
app.MapGet("/health", () => Results.Json(new { status = "ok" }));

{
    var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("IdentidadDb");
    try
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();
        await EfMigrationHistoryBaseline.MigrateWithBaselineAsync(
            db,
            historySchema: "auth",
            markerSchema: "auth",
            markerTable: "roles",
            migrationIds: ["20260510022937_InitialCreate"],
            startupLogger);
        startupLogger.LogInformation("Migraciones del esquema auth aplicadas.");
        await IdentidadRolesSeed.EnsureAsync(db);
        if (app.Environment.IsDevelopment())
        {
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            await IdentidadDevAdminSeed.EnsureAsync(
                db,
                IdentidadDevAdminSeed.DefaultLogin,
                hasher.Hash("DevAdmin123!"));
        }
    }
    catch (Exception ex)
    {
        startupLogger.LogCritical(ex,
            "No se pudo aplicar migraciones auth. Revise DATABASE_URL / ConnectionStrings:IdentidadDb en Railway.");
        throw;
    }
}

app.MapGet("/health/db", async (IdentidadDbContext db, CancellationToken ct) =>
{
    try
    {
        _ = await db.Roles.AsNoTracking().AnyAsync(ct);
        return Results.Json(new { status = "ok", schema = "auth" });
    }
    catch (Exception ex)
    {
        return Results.Json(new { status = "error", message = ex.Message }, statusCode: 503);
    }
});

app.Run();
