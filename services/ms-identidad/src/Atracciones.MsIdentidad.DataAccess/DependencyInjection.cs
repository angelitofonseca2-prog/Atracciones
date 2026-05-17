using Atracciones.MsIdentidad.DataAccess.Context;
using Atracciones.MsIdentidad.DataAccess.Repositories;
using Atracciones.MsIdentidad.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atracciones.MsIdentidad.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentidadPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("IdentidadDb")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:IdentidadDb");

        services.AddDbContext<IdentidadDbContext>(o =>
            o.UseNpgsql(cs, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "auth")));

        services.AddScoped<IIdentidadUsuarioRepository, IdentidadUsuarioRepository>();
        return services;
    }
}
