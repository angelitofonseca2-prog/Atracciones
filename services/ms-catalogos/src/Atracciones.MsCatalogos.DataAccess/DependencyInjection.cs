using Atracciones.MsCatalogos.DataAccess.Context;
using Atracciones.MsCatalogos.DataAccess.Repositories;
using Atracciones.MsCatalogos.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atracciones.MsCatalogos.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogosPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("CatalogosDb")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:CatalogosDb");
        services.AddDbContext<CatalogosDbContext>(o =>
            o.UseNpgsql(cs, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalogos")));
        services.AddScoped<ICatalogosRepository, CatalogosRepository>();
        return services;
    }
}
