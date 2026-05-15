using Atracciones.MsAtracciones.Business.Common;
using Atracciones.MsAtracciones.Business.Dtos.Public.Atracciones;

namespace Atracciones.MsAtracciones.Business.Services;

public interface IInventarioPublicAppService
{
    Task<DataPagedResult<AtraccionListadoResponse>> ListarAsync(AtraccionFiltroRequest request, string baseUrl, CancellationToken ct = default);
    Task<AtraccionDetalleResponse> ObtenerPorGuidAsync(Guid atGuid, string baseUrl, CancellationToken ct = default);
    Task<FiltrosAtraccionResponse> ObtenerFiltrosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TicketDisponibleResponse>> ListarTicketsAsync(Guid atGuid, CancellationToken ct = default);
    Task<IReadOnlyList<HorarioProximoResponse>> ListarHorariosPorTicketAsync(Guid tckGuid, CancellationToken ct = default);
    Task<IReadOnlyList<HorarioProximoResponse>> ListarHorariosDisponiblesAsync(Guid atGuid, CancellationToken ct = default);
}
