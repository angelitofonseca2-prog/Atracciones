using System.Text;
using System.Text.Json;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Api.Models.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Microservicio.Atracciones.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration config)
    {
        var jwt = config.GetSection("JwtSettings").Get<JwtSettings>()!;

        JsonWebKeySet? jwks = null;
        if (!string.IsNullOrWhiteSpace(jwt.JwksUrl))
        {
            using var hc = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var json = hc.GetStringAsync(jwt.JwksUrl).GetAwaiter().GetResult();
            jwks = new JsonWebKeySet(json);
        }

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                };

                if (jwks is not null)
                {
                    options.TokenValidationParameters.IssuerSigningKeyResolver = (_, _, kid, _) =>
                    {
                        if (string.IsNullOrEmpty(kid))
                            return jwks.GetSigningKeys();
                        return jwks.GetSigningKeys().Where(k => k.KeyId == kid);
                    };
                }
                else
                {
                    options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.SecretKey));
                }

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var errorBody = new ApiErrorResponse
                        {
                            Status = 401,
                            Error = "No autorizado",
                            Details = new List<string> { "Token inválido o expirado." },
                            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            Path = context.Request.Path.ToString(),
                        };
                        var json = JsonSerializer.Serialize(
                            errorBody,
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
                        await context.Response.WriteAsync(json);
                    },
                };
            });

        return services;
    }
}
