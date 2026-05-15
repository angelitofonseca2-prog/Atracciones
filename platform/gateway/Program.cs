using Atracciones.Platform.BuildingBlocks.Idempotency;
using Atracciones.Platform.BuildingBlocks.Middleware;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

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

app.Run();
