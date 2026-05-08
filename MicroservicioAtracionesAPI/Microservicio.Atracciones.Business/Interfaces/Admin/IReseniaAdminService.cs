using Microservicio.Atracciones.Business.DTOs.Admin.Resenias;

namespace Microservicio.Atracciones.Business.Interfaces.Admin
{
    public interface IReseniaAdminService
    {
        Task<ReseniaAdminResponse> ObtenerPorGuidAsync(Guid rsnGuid);
        Task<IReadOnlyList<ReseniaAdminResponse>> ListarPorAtraccionAsync(Guid atGuid);
        Task<ReseniaAdminResponse> ActualizarAsync(Guid rsnGuid, ActualizarReseniaRequest request, string usuarioAccion, string ip);
        Task EliminarAsync(Guid rsnGuid, string usuarioAccion, string ip);
    }
}
