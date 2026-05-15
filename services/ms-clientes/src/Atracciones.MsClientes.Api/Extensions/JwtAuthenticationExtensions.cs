using System.Text.Json;
using Atracciones.MsClientes.Api.Hosted;
using Atracciones.MsClientes.Api.Models;
using Atracciones.MsClientes.Api.Options;
using Atracciones.MsClientes.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Atracciones.MsClientes.Api.Extensions;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtFromJwks(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtValidationOptions>(configuration.GetSection(JwtValidationOptions.SectionName));
        services.AddSingleton<JwksKeyStore>();
        services.AddHostedService<JwksLoaderHostedService>();

        var jwt = configuration.GetSection(JwtValidationOptions.SectionName).Get<JwtValidationOptions>()
            ?? throw new InvalidOperationException($"Falta sección {JwtValidationOptions.SectionName}");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<JwksKeyStore>((o, store) =>
            {
                o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                    IssuerSigningKeyResolver = (_, _, kid, _) => store.ResolveSigningKeys(kid),
                };
                o.Events = new JwtBearerEvents
                {
                    OnChallenge = async ctx =>
                    {
                        ctx.HandleResponse();
                        ctx.Response.StatusCode = 401;
                        ctx.Response.ContentType = "application/json";
                        var body = new ApiErrorResponse
                        {
                            Status = 401,
                            Error = "No autorizado",
                            Details = new List<string> { "Token inválido o expirado." },
                            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            Path = ctx.Request.Path.ToString(),
                        };
                        await ctx.Response.WriteAsync(JsonSerializer.Serialize(body,
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }));
                    },
                };
            });

        services.AddAuthorization(options =>
            options.AddPolicy("ClienteAutenticado", p => p.RequireRole("CLIENTE")));

        return services;
    }
}
