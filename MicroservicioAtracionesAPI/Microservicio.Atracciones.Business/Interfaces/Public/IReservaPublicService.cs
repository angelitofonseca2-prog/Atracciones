using Microservicio.Atracciones.Business.DTOs.Public.Reservas;
using Microservicio.Atracciones.Business.DTOs.Admin.Facturas;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.Business.Interfaces.Public
{
    public interface IReservaPublicService
    {
        Task<ReservaResponse> CrearAsync(CrearReservaRequest request, Guid? usuGuid, string usuarioAccion, string ip);
        Task<ReservaResponse> ObtenerPorGuidAsync(Guid revGuid, Guid usuGuid);
        Task<DataPagedResult<ReservaResponse>> ListarPorClienteAsync(Guid usuGuid, int page, int limit);
        Task CancelarAsync(Guid revGuid, CancelarReservaRequest request, Guid usuGuid, string usuarioAccion, string ip);
        Task<FacturaResponse> ConfirmarPagoAsync(Guid revGuid, ConfirmarPagoReservaRequest request, string usuarioAccion, string ip);
    }
}
