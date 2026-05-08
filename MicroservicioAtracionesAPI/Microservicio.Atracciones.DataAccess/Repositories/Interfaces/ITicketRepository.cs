using Microservicio.Atracciones.DataAccess.Entities.Reservas;

namespace Microservicio.Atracciones.DataAccess.Repositories.Interfaces
{
    public interface ITicketRepository
    {
        Task<TicketEntity?> ObtenerPorIdAsync(int tckId);
        Task<TicketEntity?> ObtenerPorGuidAsync(Guid tckGuid);
        Task<IReadOnlyList<TicketEntity>> ListarActivosAsync();
        Task<IReadOnlyList<TicketEntity>> ListarPorAtraccionAsync(int atId);
        Task AgregarAsync(TicketEntity ticket);
        void Actualizar(TicketEntity ticket);

        // Horarios (dentro del agregado Ticket)
        Task<HorarioEntity?> ObtenerHorarioPorIdAsync(int horId);
        Task<HorarioEntity?> ObtenerHorarioPorGuidAsync(Guid horGuid);
        Task<IReadOnlyList<HorarioEntity>> ListarHorariosActivosAsync();
        Task<IReadOnlyList<HorarioEntity>> ListarHorariosPorTicketAsync(int tckId);
        Task AgregarHorarioAsync(HorarioEntity horario);
        void ActualizarHorario(HorarioEntity horario);
    }
}
