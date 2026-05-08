using Microservicio.Atracciones.Business.DTOs.Admin.Facturas;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.Business.Interfaces.Public
{
    public interface IFacturaPublicService
    {
        Task<DataPagedResult<FacturaResponse>> ListarMisFacturasAsync(Guid usuGuid, int page, int limit);
    }
}
