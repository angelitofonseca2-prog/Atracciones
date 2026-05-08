using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Reservas;

namespace Microservicio.Atracciones.Business.Rules.Admin
{
    public class TicketRules
    {
        private readonly IAtraccionDataService _atraccionService;
        private readonly ITicketDataService _ticketService;

        public TicketRules(IAtraccionDataService atraccionService, ITicketDataService ticketService)
        {
            _atraccionService = atraccionService;
            _ticketService = ticketService;
        }

        public async Task ValidarAtraccionExisteAsync(Guid atGuid)
        {
            var atraccion = await _atraccionService.ObtenerPorGuidAsync(atGuid)
                ?? throw new NotFoundException("Atraccion", atGuid);

            if (atraccion.AtEstado != 'A')
                throw new ConflictException("La atracción asociada no está activa.");
        }

        public async Task ValidarTicketExisteAsync(Guid tckGuid)
        {
            var ticket = await _ticketService.ObtenerPorGuidAsync(tckGuid)
                ?? throw new NotFoundException("Ticket", tckGuid);

            if (ticket.TckEstado != 'A')
                throw new ConflictException("El ticket no está activo.");
        }

        public static void ValidarCupos(int cuposNuevos, int capacidadMaxima)
        {
            if (cuposNuevos < 0 || cuposNuevos > capacidadMaxima)
                throw new ConflictException(
                    $"Los cupos disponibles deben estar entre 0 y la capacidad máxima ({capacidadMaxima}).");
        }
    }
}
