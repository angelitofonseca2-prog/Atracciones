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

var builder = WebApplication.CreateBuilder(args);

// En Docker se define GrpcPort (9090) → dos puertos: 8080 HTTP/1 REST + 9090 HTTP/2 gRPC h2c.
// En local se omite GrpcPort y se usa ASPNETCORE_URLS con Http1AndHttp2.
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

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();
    await db.Database.MigrateAsync();
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

app.Run();
