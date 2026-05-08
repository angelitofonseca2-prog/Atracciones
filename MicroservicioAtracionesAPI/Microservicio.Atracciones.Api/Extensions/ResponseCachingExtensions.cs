using Microsoft.AspNetCore.Mvc;
using Microservicio.Atracciones.Api.Helpers;
using Microservicio.Atracciones.Api.Models.Settings;

namespace Microservicio.Atracciones.Api.Extensions
{
    public static class ResponseCachingExtensions
    {
        public static IServiceCollection AddResponseCachingConfig(
            this IServiceCollection services, IConfiguration config)
        {
            var cache = config.GetSection("CacheSettings").Get<CacheSettings>()
                        ?? new CacheSettings();

            services.AddResponseCaching();

            // Registra los perfiles de caché sin llamar AddControllers() nuevamente
            // para no interferir con la configuración de filtros/serialización de Program.cs
            services.Configure<MvcOptions>(options =>
            {
                options.CacheProfiles.Add(CacheProfileNames.Listado, new CacheProfile
                {
                    Duration = cache.ListadoTtlSegundos,
                    Location = ResponseCacheLocation.Any,
                    VaryByQueryKeys = new[] { "ciudad", "tipo", "subtipo", "etiqueta", "idioma", "calificacion_min", "horario", "disponible", "ordenar_por", "page", "limit" }
                });
                options.CacheProfiles.Add(CacheProfileNames.Detalle, new CacheProfile
                {
                    Duration = cache.DetalleTtlSegundos,
                    Location = ResponseCacheLocation.Any
                });
                options.CacheProfiles.Add(CacheProfileNames.Filtros, new CacheProfile
                {
                    Duration = cache.FiltrosTtlSegundos,
                    Location = ResponseCacheLocation.Any,
                    VaryByQueryKeys = new[] { "ciudad" }
                });
                options.CacheProfiles.Add(CacheProfileNames.SinCache, new CacheProfile
                {
                    NoStore = true
                });
            });

            return services;
        }
    }
}
