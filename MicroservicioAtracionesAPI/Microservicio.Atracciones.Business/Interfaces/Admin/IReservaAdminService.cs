using Microservicio.Atracciones.Business.DTOs.Admin.Reservas;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.Business.Interfaces.Admin
{
    public interface IReservaAdminService
    {
        Task<ReservaAdminResponse> ObtenerPorGuidAsync(Guid revGuid);
        Task<DataPagedResult<ReservaAdminResponse>> ListarAsync(ReservaAdminFiltroRequest filtro);
        Task ActualizarEstadoAsync(Guid revGuid, ActualizarEstadoReservaRequest request, string usuarioAccion, string ip);
    }
}
