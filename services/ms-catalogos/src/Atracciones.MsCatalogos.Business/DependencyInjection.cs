using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atracciones.MsCatalogos.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogosBusiness(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MonolithCatalogLegacySyncOptions>(
            configuration.GetSection(MonolithCatalogLegacySyncOptions.SectionName));
        services.AddScoped<ICatalogosAdminAppService, CatalogosAdminAppService>();
        return services;
    }
}
