using Microservicio.Atracciones.Api.Helpers;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Api.Models.Settings;
using Microservicio.Atracciones.Api.Services;
using System.Text.Json;

namespace Microservicio.Atracciones.Api.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCorsPolicy(
            this IServiceCollection services, IConfiguration config)
        {
            var cors = config.GetSection("CorsSettings").Get<CorsSettings>()
                       ?? new CorsSettings();

            services.AddCors(options =>
            {
                options.AddPolicy(cors.PolicyName, builder =>
                {
                    if (cors.AllowedOrigins.Length == 0)
                        builder.AllowAnyOrigin();
                    else
                        builder.WithOrigins(cors.AllowedOrigins);

                    builder
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            return services;
        }
    }
}
