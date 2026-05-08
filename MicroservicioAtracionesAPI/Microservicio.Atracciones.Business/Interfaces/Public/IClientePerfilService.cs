using Microservicio.Atracciones.Business.DTOs.Public.Clientes;

namespace Microservicio.Atracciones.Business.Interfaces.Public
{
    public interface IClientePerfilService
    {
        Task<PerfilClienteResponse> ObtenerAsync(Guid usuGuid);
        Task<PerfilClienteResponse> ActualizarAsync(Guid usuGuid, ActualizarPerfilClienteRequest request);
    }
}
