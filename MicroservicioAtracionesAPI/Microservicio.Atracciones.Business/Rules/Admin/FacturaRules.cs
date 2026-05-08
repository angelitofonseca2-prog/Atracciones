using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.DataManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.Rules.Admin
{
    public class FacturaRules
    {
        private readonly IReservaDataService _reservaService;
        private readonly IFacturaDataService _facturaService;

        public FacturaRules(IReservaDataService reservaService, IFacturaDataService facturaService)
        {
            _reservaService = reservaService;
            _facturaService = facturaService;
        }

        public async Task ValidarPuedeFacturarAsync(Guid revGuid)
        {
            var reserva = await _reservaService.ObtenerPorGuidAsync(revGuid)
                ?? throw new NotFoundException("Reserva", revGuid);

            if (reserva.RevEstado == 'C')
                throw new ConflictException("No se puede generar factura para una reserva cancelada.");

            var facturaExistente = await _facturaService.ObtenerPorReservaAsync(reserva.RevId);
            if (facturaExistente is not null)
                throw new ConflictException("Esta reserva ya tiene una factura generada.");
        }

        public static string GenerarNumeroFactura()
        {
            var ts = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var rand = new Random().Next(1000, 9999);
            return $"FAC-{ts}-{rand}";
        }
    }
}
