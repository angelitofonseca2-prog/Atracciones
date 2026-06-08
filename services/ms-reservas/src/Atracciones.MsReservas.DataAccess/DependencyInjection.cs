using Atracciones.MsReservas.DataAccess.Context;
using Atracciones.MsReservas.DataAccess.Repositories;
using Atracciones.MsReservas.DataManagement.Interfaces;
using Atracciones.Platform.BuildingBlocks.EventBus.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atracciones.MsReservas.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddVentasPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("VentasDb")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:VentasDb");
        services.AddDbContext<VentasDbContext>(o =>
            o.UseNpgsql(cs, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ventas")));
        services.AddScoped<IReservaRepository, ReservaRepository>();
        services.AddScoped<IOutboxWriter, OutboxRepository>();
        services.AddScoped<IOutboxReader, OutboxRepository>();
        services.AddScoped<IProcessedEventStore, OutboxRepository>();
        services.AddScoped<IMarketplaceSeguimientoRepository, MarketplaceSeguimientoRepository>();

        // CRM (clientes) en el mismo PostgreSQL que ventas (schema crm)
        var crmCs = configuration.GetConnectionString("CrmDb") ?? cs;
        services.AddDbContext<CrmDbContext>(o =>
            o.UseNpgsql(crmCs, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "crm")));
        services.AddScoped<IClienteRepository, ClienteRepository>();

        return services;
    }
}
