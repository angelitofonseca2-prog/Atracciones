using Microservicio.Atracciones.Business.DTOs.Admin.Facturas;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.Business.Interfaces.Admin
{
    public interface IFacturaAdminService
    {
        Task<FacturaResponse> GenerarAsync(GenerarFacturaRequest request, string usuarioAccion, string ip);
        Task<FacturaResponse> ObtenerPorGuidAsync(Guid facGuid);
        Task<DataPagedResult<FacturaResponse>> ListarAsync(int page, int limit);
    }
}
