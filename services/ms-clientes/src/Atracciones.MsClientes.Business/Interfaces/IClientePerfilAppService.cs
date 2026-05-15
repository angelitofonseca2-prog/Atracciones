using Atracciones.MsClientes.Business.DTOs;

namespace Atracciones.MsClientes.Business.Interfaces;

public interface IClientePerfilAppService
{
    Task<PerfilClienteResponse> ObtenerAsync(Guid usuGuid, CancellationToken ct = default);
    Task<PerfilClienteResponse> ActualizarAsync(Guid usuGuid, ActualizarPerfilClienteRequest request, CancellationToken ct = default);
}
