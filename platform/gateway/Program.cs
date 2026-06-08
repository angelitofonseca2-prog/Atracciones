using Atracciones.Platform.BuildingBlocks.Idempotency;
using Atracciones.Platform.BuildingBlocks.Middleware;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Permite override de destinos YARP vía variable de entorno ASPNETCORE_ENVIRONMENT=Railway
// o archivos appsettings.Railway.json cuando se despliega en Railway.
builder.Configuration
    .AddJsonFile("appsettings.Railway.json", optional: true, reloadOnChange: false);

// Las variables de entorno sobreescriben la configuración anterior.
// En Railway: ReverseProxy__Clusters__reservas__Destinations__d1__Address=http://...
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var corsSection = builder.Configuration.GetSection("Cors");
var origins = corsSection.Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("GatewayCors", policy =>
    {
        if (origins.Length == 0)
            policy.AllowAnyOrigin();
        else
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    });
});

var otlp = builder.Configuration["Otlp:Endpoint"];
if (!string.IsNullOrWhiteSpace(otlp))
{
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService(
            serviceName: builder.Configuration["Otlp:ServiceName"] ?? "atracciones-gateway",
            serviceVersion: "1.0.0"))
        .WithTracing(t => t
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter(o => o.Endpoint = new Uri(otlp)));
}

var app = builder.Build();

app.Use(async (ctx, next) =>
{
    ctx.Response.OnStarting(() =>
    {
        var p = ctx.Request.Path;
        if (p.StartsWithSegments("/api") || p.StartsWithSegments("/health"))
        {
            ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
            ctx.Response.Headers["X-Frame-Options"] = "DENY";
            ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            ctx.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
        }

        return Task.CompletedTask;
    });
    await next();
});

app.UseCorrelationId();
app.UseIdempotencyKeyProbe();
app.UseCors("GatewayCors");
app.MapReverseProxy();

// Health endpoint expuesto directamente en el gateway para Railway
app.MapGet("/health", () => Results.Json(new { status = "ok", service = "gateway" }));

app.Run();
