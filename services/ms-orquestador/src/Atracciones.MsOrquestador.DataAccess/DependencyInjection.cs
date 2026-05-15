using Atracciones.MsOrquestador.DataAccess.Context;
using Atracciones.MsOrquestador.DataAccess.Repositories;
using Atracciones.MsOrquestador.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atracciones.MsOrquestador.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddOrquestadorPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("OrquestadorDb")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:OrquestadorDb");
        services.AddDbContext<OrquestadorDbContext>(o =>
            o.UseNpgsql(cs, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "orq")));
        services.AddScoped<ISagaRepository, SagaRepository>();
        services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
        services.AddScoped<IPayPalPaymentRepository, PayPalPaymentRepository>();
        return services;
    }
}
