using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Reservas;

namespace Microservicio.Atracciones.Business.Rules.Admin
{
    public class AtraccionRules
    {
        private readonly IDestinoDataService _destinoService;
        private readonly IAtraccionDataService _atraccionService;

        public AtraccionRules(IDestinoDataService destinoService, IAtraccionDataService atraccionService)
        {
            _destinoService = destinoService;
            _atraccionService = atraccionService;
        }

        public async Task ValidarDestinoExisteAsync(Guid desGuid)
        {
            var destino = await _destinoService.ObtenerPorGuidAsync(desGuid)
                ?? throw new NotFoundException("Destino", desGuid);

            if (destino.DesEstado != 'A')
                throw new ConflictException("El destino seleccionado no está activo.");
        }

        public async Task ValidarExisteAsync(Guid atGuid)
        {
            var atraccion = await _atraccionService.ObtenerPorGuidAsync(atGuid)
                ?? throw new NotFoundException("Atraccion", atGuid);

            if (atraccion.AtEstado != 'A')
                throw new ConflictException("La atracción no está activa.");
        }
    }
}
