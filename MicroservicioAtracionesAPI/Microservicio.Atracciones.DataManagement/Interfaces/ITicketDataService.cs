using Microservicio.Atracciones.DataManagement.Models.Reservas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Interfaces
{
    public interface ITicketDataService
    {
        Task<TicketDataModel?> ObtenerPorIdAsync(int tckId);
        Task<TicketDataModel?> ObtenerPorGuidAsync(Guid tckGuid);
        Task<IReadOnlyList<TicketDataModel>> ListarAsync();
        Task<IReadOnlyList<TicketDataModel>> ListarPorAtraccionAsync(int atId);
        Task CrearAsync(TicketDataModel model);
        Task ActualizarAsync(TicketDataModel model);
        Task EliminarLogicoAsync(int tckId, string usuarioAccion, string ip);
        Task<HorarioDataModel?> ObtenerHorarioPorIdAsync(int horId);

        // Horarios
        Task<HorarioDataModel?> ObtenerHorarioPorGuidAsync(Guid horGuid);
        Task<IReadOnlyList<HorarioDataModel>> ListarHorariosAsync();
        Task<IReadOnlyList<HorarioDataModel>> ListarHorariosPorTicketAsync(int tckId);
        Task<IReadOnlyList<HorarioDataModel>> ListarHorariosPorAtraccionAsync(int atId, int diasAdelante = 7);
        Task CrearHorarioAsync(HorarioDataModel model);
        Task ActualizarHorarioAsync(HorarioDataModel model);
        Task EliminarHorarioLogicoAsync(int horId, string usuarioAccion, string ip);
        Task<bool> TieneReservasActivasPorTicketAsync(int tckId);
        Task<bool> TieneReservasActivasPorHorarioAsync(int horId);

        // Disponibilidad en tiempo real
        Task<(bool DisponibleHoy, DateOnly? ProximaFecha, int? CuposProximos)>
            ObtenerDisponibilidadAsync(int atId);

        // #4: Batch — una sola query en lugar de N queries independientes
        Task<Dictionary<int, (bool DisponibleHoy, DateOnly? ProximaFecha, int? CuposProximos)>>
            ObtenerDisponibilidadBatchAsync(IEnumerable<int> atIds);

        Task<(bool HayCupos, int CuposActuales)>
            VerificarCuposAsync(int horId, int cantidadSolicitada);
    }
}
