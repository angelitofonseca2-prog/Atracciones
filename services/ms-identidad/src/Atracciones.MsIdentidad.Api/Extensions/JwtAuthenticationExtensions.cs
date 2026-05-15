using System.Security.Claims;
using System.Text.Json;
using Atracciones.MsIdentidad.Api.Models;
using Atracciones.MsIdentidad.Api.Options;
using Atracciones.MsIdentidad.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Atracciones.MsIdentidad.Api.Extensions;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddIdentidadJwtBearer(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IdentidadSigningKeys, IOptions<JwtIssuerOptions>>((o, keys, jwt) =>
            {
                var j = jwt.Value;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = j.Issuer,
                    ValidAudience = j.Audience,
                    IssuerSigningKey = keys.SigningKey,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role,
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
                            Path = ctx.Request.Path.ToString(),
                        };
                        await ctx.Response.WriteAsync(JsonSerializer.Serialize(body,
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }));
                    },
                    OnForbidden = async ctx =>
                    {
                        ctx.Response.StatusCode = 403;
                        ctx.Response.ContentType = "application/json";
                        var body = new ApiErrorResponse
                        {
                            Status = 403,
                            Error = "Prohibido",
                            Details = new List<string> { "Se requiere rol ADMIN." },
                            Path = ctx.Request.Path.ToString(),
                        };
                        await ctx.Response.WriteAsync(JsonSerializer.Serialize(body,
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }));
                    },
                };
            });

        services.AddAuthorization(options =>
            options.AddPolicy("SoloAdmin", p => p.RequireRole("ADMIN")));

        return services;
    }
}
