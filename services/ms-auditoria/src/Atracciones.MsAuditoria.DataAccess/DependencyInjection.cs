using Atracciones.MsAuditoria.DataAccess.Context;
using Atracciones.MsAuditoria.DataAccess.Repositories;
using Atracciones.MsAuditoria.DataManagement.Interfaces;
using Atracciones.Platform.BuildingBlocks.EventBus.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atracciones.MsAuditoria.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddAuditoriaPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("AuditoriaDb")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:AuditoriaDb");
        services.AddDbContext<AuditoriaDbContext>(o =>
            o.UseNpgsql(cs, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "audit")));
        services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
        services.AddScoped<IProcessedEventStore, AuditoriaProcessedEventRepository>();
        return services;
    }
}
