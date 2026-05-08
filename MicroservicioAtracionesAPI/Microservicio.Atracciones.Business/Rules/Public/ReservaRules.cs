using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Reservas;

namespace Microservicio.Atracciones.Business.Rules.Public
{
    public class ReservaRules
    {
        private readonly ITicketDataService _ticketService;
        private readonly IReservaDataService _reservaService;

        public ReservaRules(ITicketDataService ticketService, IReservaDataService reservaService)
        {
            _ticketService = ticketService;
            _reservaService = reservaService;
        }

        /// <summary>
        /// Verifica que el horario existe, está activo y tiene cupos
        /// suficientes para el total de personas solicitadas.
        /// </summary>
        public async Task ValidarDisponibilidadAsync(Guid horGuid, int totalPersonas)
        {
            var horario = await _ticketService.ObtenerHorarioPorGuidAsync(horGuid)
                ?? throw new NotFoundException("Horario", horGuid);

            if (horario.HorEstado != 'A')
                throw new ConflictException("El horario seleccionado no está disponible.");

            var (hayCupos, cuposActuales) = await _ticketService.VerificarCuposAsync(horario.HorId, totalPersonas);

            if (!hayCupos)
                throw new ConflictException(
                    $"No hay cupos suficientes. Cupos disponibles: {cuposActuales}. Solicitados: {totalPersonas}.");
        }

        /// <summary>
        /// Verifica que cada ticket existe, pertenece al mismo horario y
        /// retorna el precio actual para congelarlo en RESERVA_DETALLE.
        /// </summary>
        public async Task<IList<(TicketDataModel Ticket, int Cantidad)>>
            ValidarYObtenerTicketsAsync(Guid horGuid, Guid atGuid, IList<(Guid TckGuid, int Cantidad)> lineas)
        {
            var resultado = new List<(TicketDataModel, int)>();
            var horario = await _ticketService.ObtenerHorarioPorGuidAsync(horGuid)
                ?? throw new NotFoundException("Horario", horGuid);

            var ticketHorario = await _ticketService.ObtenerPorIdAsync(horario.TckId)
                ?? throw new ConflictException("El ticket asociado al horario no fue encontrado.");

            foreach (var (tckGuid, cantidad) in lineas)
            {
                var ticket = await _ticketService.ObtenerPorGuidAsync(tckGuid)
                    ?? throw new NotFoundException("Ticket", tckGuid);

                if (ticket.TckEstado != 'A')
                    throw new ConflictException($"El ticket '{ticket.TckTitulo}' no está activo.");

                if (ticket.AtId != ticketHorario.AtId)
                    throw new ConflictException($"El ticket '{ticket.TckTitulo}' no pertenece a la atracción del horario seleccionado.");

                resultado.Add((ticket, cantidad));
            }

            return resultado;
        }

        /// <summary>
        /// Calcula subtotal, IVA (15%) y total de la reserva.
        /// </summary>
        public static (decimal Subtotal, decimal Iva, decimal Total) CalcularTotales(
            IList<(TicketDataModel Ticket, int Cantidad)> lineas)
        {
            var subtotal = lineas.Sum(l => l.Ticket.TckPrecio * l.Cantidad);
            var iva = Math.Round(subtotal * 0.15m, 2);
            var total = subtotal + iva;
            return (subtotal, iva, total);
        }

        /// <summary>
        /// Genera un código único de reserva con formato RES-{timestamp}{random}.
        /// </summary>
        public static string GenerarCodigo()
        {
            var ts = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var rand = Random.Shared.Next(100, 999);   // #9: evita colisiones — Random.Shared es thread-safe
            return $"RES-{ts}{rand}";
        }
    }
}
