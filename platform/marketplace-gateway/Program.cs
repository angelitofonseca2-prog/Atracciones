using Atracciones.MarketplaceGateway.GraphQL;
using Atracciones.MarketplaceGateway.Options;
using Atracciones.MarketplaceGateway.Services;
using Atracciones.Platform.BuildingBlocks.EventBus.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.Configure<ServicesOptions>(builder.Configuration.GetSection(ServicesOptions.SectionName));
builder.Services.AddHttpClient<AtraccionesProxyService>();
builder.Services.AddHttpClient<ReservasProxyService>();
builder.Services.AddSingleton<MarketplaceReservaPublisher>();
builder.Services.AddAtraccionesEventBus(builder.Configuration, services =>
{
    // Consumer que traduce eventos RabbitMQ → subscriptions HotChocolate.
    services.AddHostedService<ReservaEstadoEventConsumer>();
});

var corsOrigins = builder.Configuration.GetSection("Cors").Get<string[]>() ?? ["http://localhost:5173"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddInMemorySubscriptions();
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddInMemorySubscriptions()
    .ModifyRequestOptions(o => o.IncludeExceptionDetails = builder.Environment.IsDevelopment());

var app = builder.Build();

app.UseCors();
app.UseWebSockets();
app.Use(async (ctx, next) =>
{
    var corr = ctx.Request.Headers["X-Correlation-ID"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(corr))
        corr = Guid.NewGuid().ToString("D");
    ctx.Items["correlationId"] = corr;
    ctx.Response.Headers["X-Correlation-ID"] = corr;
    await next();
});

app.MapGraphQL("/graphql");
app.MapGet("/health", () => Results.Json(new { status = "ok" }));

app.Run();
