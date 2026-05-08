using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Reservas;
namespace Microservicio.Atracciones.Business.Rules.Public
{
    public class ReseniaRules
    {
        private readonly IReservaDataService _reservaService;
        private readonly IReseniaDataService _reseniaService;

        public ReseniaRules(IReservaDataService reservaService, IReseniaDataService reseniaService)
        {
            _reservaService = reservaService;
            _reseniaService = reseniaService;
        }

        public async Task ValidarPuedeResenarAsync(Guid revGuid, int cliId)
        {
            var reserva = await _reservaService.ObtenerPorGuidAsync(revGuid)
                ?? throw new NotFoundException("Reserva", revGuid);

            if (reserva.CliId != cliId)
                throw new ForbiddenBusinessException("No puedes reseñar una reserva que no es tuya.");

            if (reserva.RevEstado != 'A')
                throw new ConflictException("Solo se puede reseñar una reserva activa o confirmada.");

            var yaExiste = await _reseniaService.YaTieneReseniaAsync(reserva.RevId);
            if (yaExiste)
                throw new ConflictException("Esta reserva ya tiene una reseña registrada.");
        }

        public async Task ValidarNoHaResenadoAtraccionAsync(int cliId, int atId)
        {
            var yaExiste = await _reseniaService.YaTieneReseniaParaAtraccionAsync(cliId, atId);
            if (yaExiste)
                throw new ConflictException("Ya registraste una reseña para esta atracción.");
        }
    }
}
