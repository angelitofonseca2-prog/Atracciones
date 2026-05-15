using Atracciones.MsAtracciones.DataAccess.Context;
using Atracciones.MsAtracciones.DataAccess.Repositories;
using Atracciones.MsAtracciones.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atracciones.MsAtracciones.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddInventarioPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("InventarioDb")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:InventarioDb");
        services.AddDbContext<InventarioDbContext>(o =>
            o.UseNpgsql(cs, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "inventario")));
        services.AddScoped<IInventarioRepository, InventarioRepository>();
        services.AddScoped<IReseniaRepository, ReseniaRepository>();

        // Catálogos en el mismo PostgreSQL que inventario (schema catalogos)
        var catalogCs = configuration.GetConnectionString("CatalogosDb") ?? cs;
        services.AddDbContext<CatalogosDbContext>(o =>
            o.UseNpgsql(catalogCs, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalogos")));
        services.AddScoped<ICatalogosRepository, CatalogosRepository>();

        return services;
    }
}
