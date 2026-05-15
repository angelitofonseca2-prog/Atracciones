using Atracciones.MsFacturacion.DataAccess.Context;
using Atracciones.MsFacturacion.DataAccess.Repositories;
using Atracciones.MsFacturacion.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atracciones.MsFacturacion.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddBillingPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("BillingDb")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:BillingDb");
        services.AddDbContext<BillingDbContext>(o =>
            o.UseNpgsql(cs, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "billing")));
        services.AddScoped<IFacturaRepository, FacturaRepository>();
        return services;
    }
}
