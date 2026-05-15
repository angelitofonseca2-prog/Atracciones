using Atracciones.MsClientes.Business.Interfaces;
using Atracciones.MsClientes.Business.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Atracciones.MsClientes.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddClientesBusiness(this IServiceCollection services)
    {
        services.AddScoped<IClientePerfilAppService, ClientePerfilAppService>();
        return services;
    }
}
