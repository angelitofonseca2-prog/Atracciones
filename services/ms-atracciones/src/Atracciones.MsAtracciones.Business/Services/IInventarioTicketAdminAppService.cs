using Atracciones.MsAtracciones.Business.Dtos.Admin.Tickets;

namespace Atracciones.MsAtracciones.Business.Services;

public interface IInventarioTicketAdminAppService
{
    Task<IReadOnlyList<TicketResponse>> ListarTicketsAsync(CancellationToken ct = default);
    Task<TicketResponse> ObtenerTicketPorGuidAsync(Guid tckGuid, CancellationToken ct = default);
    Task<IReadOnlyList<TicketResponse>> ListarTicketsPorAtraccionAsync(Guid atGuid, CancellationToken ct = default);
    Task<TicketResponse> CrearTicketAsync(CrearTicketRequest request, string usuario, string ip, CancellationToken ct = default);
    Task<TicketResponse> ActualizarTicketAsync(Guid tckGuid, ActualizarTicketRequest request, string usuario, string ip, CancellationToken ct = default);
    Task EliminarTicketAsync(Guid tckGuid, string usuario, string ip, CancellationToken ct = default);

    Task<IReadOnlyList<HorarioResponse>> ListarHorariosAsync(CancellationToken ct = default);
    Task<HorarioResponse> ObtenerHorarioPorGuidAsync(Guid horGuid, CancellationToken ct = default);
    Task<IReadOnlyList<HorarioResponse>> ListarHorariosPorTicketAsync(Guid tckGuid, CancellationToken ct = default);
    Task<IReadOnlyList<HorarioResponse>> ListarHorariosPorAtraccionAsync(Guid atGuid, CancellationToken ct = default);
    Task<HorarioResponse> CrearHorarioAsync(CrearHorarioRequest request, string usuario, string ip, CancellationToken ct = default);
    Task<HorarioResponse> ActualizarHorarioAsync(Guid horGuid, ActualizarHorarioRequest request, string usuario, string ip, CancellationToken ct = default);
    Task EliminarHorarioAsync(Guid horGuid, string usuario, string ip, CancellationToken ct = default);
}
