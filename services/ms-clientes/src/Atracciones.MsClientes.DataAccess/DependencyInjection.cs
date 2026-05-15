using Atracciones.MsClientes.DataAccess.Context;
using Atracciones.MsClientes.DataAccess.Repositories;
using Atracciones.MsClientes.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atracciones.MsClientes.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddCrmPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("CrmDb")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:CrmDb");
        services.AddDbContext<CrmDbContext>(o =>
            o.UseNpgsql(cs, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "crm")));
        services.AddScoped<IClienteRepository, ClienteRepository>();
        return services;
    }
}
