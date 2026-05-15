using System.Text.Json;
using Atracciones.MsClientes.Api.Extensions;
using Atracciones.MsClientes.Api.Grpc;
using Atracciones.MsClientes.Api.Middleware;
using Atracciones.MsClientes.Api.Options;
using Atracciones.MsClientes.Business;
using Atracciones.MsClientes.DataAccess;
using Atracciones.MsClientes.DataAccess.Context;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddCrmPersistence(builder.Configuration);
builder.Services.AddClientesBusiness();
builder.Services.AddJwtFromJwks(builder.Configuration);
builder.Services.AddGrpc();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<ClienteGrpcService>();
app.MapGet("/health", () => Results.Json(new { status = "ok" }));

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
