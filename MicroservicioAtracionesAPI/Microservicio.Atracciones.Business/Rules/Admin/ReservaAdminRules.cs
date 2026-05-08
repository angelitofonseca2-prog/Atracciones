using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.DataManagement.Interfaces;


namespace Microservicio.Atracciones.Business.Rules.Admin
{
    public class ReservaAdminRules
    {
        private readonly IReservaDataService _reservaService;

        public ReservaAdminRules(IReservaDataService reservaService)
            => _reservaService = reservaService;

        public async Task ValidarCambioEstadoAsync(Guid revGuid, char nuevoEstado)
        {
            var reserva = await _reservaService.ObtenerPorGuidAsync(revGuid)
                ?? throw new NotFoundException("Reserva", revGuid);

            // No se puede cambiar estado de una reserva ya cancelada
            if (reserva.RevEstado == 'C')
                throw new ConflictException("No se puede modificar el estado de una reserva cancelada.");

            // No se puede poner activa una reserva que ya fue inactivada si hay tickets agotados
            if (reserva.RevEstado == 'I' && nuevoEstado == 'A')
                throw new ConflictException("No se puede reactivar una reserva inactivada directamente.");
        }
    }
}
