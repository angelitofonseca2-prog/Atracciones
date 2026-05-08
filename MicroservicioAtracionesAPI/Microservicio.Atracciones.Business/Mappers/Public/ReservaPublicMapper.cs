using Microservicio.Atracciones.Business.DTOs.Public.Reservas;
using Microservicio.Atracciones.DataManagement.Models.Reservas;

namespace Microservicio.Atracciones.Business.Mappers.Public
{
    public static class ReservaPublicMapper
    {
        public static ReservaResponse ToResponse(ReservaDataModel model, HorarioDataModel horario, string atraccionNombre)
            => new()
            {
                RevGuid = model.RevGuid.ToString(),
                RevCodigo = model.RevCodigo,
                HorFecha = horario.HorFecha.ToString("yyyy-MM-dd"),
                HorHoraInicio = horario.HorHoraInicio.ToString("HH:mm"),
                HorHoraFin = horario.HorHoraFin?.ToString("HH:mm"),
                AtraccionNombre = atraccionNombre,
                RevSubtotal = model.RevSubtotal,
                RevValorIva = model.RevValorIva,
                RevTotal = model.RevTotal,
                Moneda = "USD",
                RevEstado = model.RevEstado.ToString(),
                RevFechaReservaUtc = model.RevFechaReservaUtc,
                Detalle = model.Detalle.Select(d => new ReservaDetalleResponse
                {
                    TckTipoParticipante = d.TckTipoParticipante,
                    Cantidad = d.RdetCantidad,
                    PrecioUnit = d.RdetPrecioUnit,
                    Subtotal = d.RdetSubtotal
                }).ToList()
            };
    }
}
