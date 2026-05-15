using Atracciones.Contracts.Auditoria.V1;
using Atracciones.Contracts.Clientes.V1;
using Atracciones.Contracts.Facturacion.V1;
using Atracciones.Contracts.Identidad.V1;
using Atracciones.Contracts.Inventario.V1;
using Atracciones.Contracts.Reservas.V1;
using Atracciones.MsOrquestador.Business.Integration;
using Atracciones.MsOrquestador.Business.Options;
using Atracciones.MsOrquestador.Business.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atracciones.MsOrquestador.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddOrquestadorBusiness(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GrpcClientsOptions>(configuration.GetSection(GrpcClientsOptions.SectionName));
        services.AddSingleton<GrpcChannelHolder>();

        services.AddSingleton(sp =>
            new UsuarioService.UsuarioServiceClient(sp.GetRequiredService<GrpcChannelHolder>().Identidad));
        services.AddSingleton(sp =>
            new ClienteService.ClienteServiceClient(sp.GetRequiredService<GrpcChannelHolder>().Clientes));
        services.AddSingleton(sp =>
            new AtraccionInventarioService.AtraccionInventarioServiceClient(sp.GetRequiredService<GrpcChannelHolder>().Atracciones));
        services.AddSingleton(sp =>
            new ReservaService.ReservaServiceClient(sp.GetRequiredService<GrpcChannelHolder>().Reservas));
        services.AddSingleton(sp =>
            new FacturaService.FacturaServiceClient(sp.GetRequiredService<GrpcChannelHolder>().Facturacion));
        services.AddSingleton(sp =>
            new AuditoriaService.AuditoriaServiceClient(sp.GetRequiredService<GrpcChannelHolder>().Auditoria));

        services.AddSingleton<AuditoriaBestEffortPublisher>();
        services.AddHttpClient("identidad");

        services.Configure<PayPalOptions>(configuration.GetSection(PayPalOptions.SectionName));
        services.AddHttpClient("paypal");
        services.AddSingleton<PayPalApiClient>();
        services.AddScoped<IPayPalPagosService, PayPalPagosAppService>();

        services.AddScoped<IReservaOrquestacionService, ReservaOrquestacionAppService>();
        services.AddScoped<IRegistroOrquestacionService, RegistroOrquestacionAppService>();
        return services;
    }
}
