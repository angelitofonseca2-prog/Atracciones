using Atracciones.MsIdentidad.Business.Interfaces;
using Atracciones.MsIdentidad.Business.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Atracciones.MsIdentidad.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentidadBusiness(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioProvisioningService, UsuarioProvisioningService>();
        return services;
    }
}
