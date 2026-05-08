using Microservicio.Atracciones.Business.DTOs.Admin.Atracciones;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.Business.Interfaces.Admin
{
    public interface IAtraccionAdminService
    {
        Task<AtraccionAdminResponse> CrearAsync(CrearAtraccionRequest request, string usuarioAccion, string ip);
        Task<AtraccionAdminResponse> ActualizarAsync(Guid atGuid, ActualizarAtraccionRequest request, string usuarioAccion, string ip);
        Task EliminarAsync(Guid atGuid, string usuarioAccion, string ip);
        Task<AtraccionAdminResponse> ObtenerPorGuidAsync(Guid atGuid);
        Task<DataPagedResult<AtraccionAdminResponse>> ListarAsync(AtraccionAdminFiltroRequest filtro);
    }
}
