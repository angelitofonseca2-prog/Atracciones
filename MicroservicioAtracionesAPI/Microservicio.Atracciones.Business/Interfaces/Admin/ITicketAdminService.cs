using Microservicio.Atracciones.Business.DTOs.Admin.Tickets;

namespace Microservicio.Atracciones.Business.Interfaces.Admin
{
    public interface ITicketAdminService
    {
        Task<IReadOnlyList<TicketResponse>> ListarTicketsAsync();
        Task<TicketResponse> ObtenerTicketPorGuidAsync(Guid tckGuid);
        Task<IReadOnlyList<TicketResponse>> ListarTicketsPorAtraccionAsync(Guid atGuid);
        Task<TicketResponse> CrearTicketAsync(CrearTicketRequest request, string usuarioAccion, string ip);
        Task<TicketResponse> ActualizarTicketAsync(Guid tckGuid, ActualizarTicketRequest request, string usuarioAccion, string ip);
        Task EliminarTicketAsync(Guid tckGuid, string usuarioAccion, string ip);
        Task<IReadOnlyList<HorarioResponse>> ListarHorariosAsync();
        Task<HorarioResponse> ObtenerHorarioPorGuidAsync(Guid horGuid);
        Task<IReadOnlyList<HorarioResponse>> ListarHorariosPorTicketAsync(Guid tckGuid);
        Task<IReadOnlyList<HorarioResponse>> ListarHorariosPorAtraccionAsync(Guid atGuid);
        Task<HorarioResponse> CrearHorarioAsync(CrearHorarioRequest request, string usuarioAccion, string ip);
        Task<HorarioResponse> ActualizarHorarioAsync(Guid horGuid, ActualizarHorarioRequest request, string usuarioAccion, string ip);
        Task EliminarHorarioAsync(Guid horGuid, string usuarioAccion, string ip);
    }
}
