using Microservicio.Atracciones.Business.DTOs.Admin.Destinos;

namespace Microservicio.Atracciones.Business.Interfaces.Admin
{
    public interface IDestinoAdminService
    {
        Task<DestinoResponse> CrearAsync(CrearDestinoRequest request, string usuarioAccion, string ip);
        Task<DestinoResponse> ActualizarAsync(Guid desGuid, ActualizarDestinoRequest request, string usuarioAccion, string ip);
        Task EliminarAsync(Guid desGuid, string usuarioAccion, string ip);
        Task<IReadOnlyList<DestinoResponse>> ListarAsync();
    }
}
