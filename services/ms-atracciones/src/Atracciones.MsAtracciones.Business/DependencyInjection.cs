using Atracciones.MsAtracciones.Business.Integration;
using Atracciones.MsAtracciones.Business.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atracciones.MsAtracciones.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddInventarioBusiness(this IServiceCollection services, IConfiguration configuration)
    {
        // CatalogoLocalClient resuelve catálogos en proceso (misma BD), sin red gRPC
        services.AddScoped<ICatalogoGrpcClient, CatalogoLocalClient>();

        services.AddScoped<IInventarioPublicAppService, InventarioPublicAppService>();
        services.AddScoped<IInventarioAdminAppService, InventarioAdminAppService>();
        services.AddScoped<IInventarioTicketAdminAppService, InventarioTicketAdminAppService>();
        services.AddScoped<IInventarioCupoAppService, InventarioCupoAppService>();

        services.AddScoped<ICatalogosAdminAppService, CatalogosAdminAppService>();
        services.AddScoped<IReseniaAppService, ReseniaAppService>();

        return services;
    }
}
