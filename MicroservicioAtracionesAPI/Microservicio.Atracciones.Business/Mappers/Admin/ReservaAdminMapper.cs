using Microservicio.Atracciones.Business.DTOs.Admin.Reservas;
using Microservicio.Atracciones.DataManagement.Models.Reservas;
using System.Globalization;

namespace Microservicio.Atracciones.Business.Mappers.Admin
{
    public static class ReservaAdminMapper
    {
        public static ReservaAdminResponse ToResponse(ReservaDataModel model, string clienteNombre, string atraccionNombre)
            => new()
            {
                RevGuid = model.RevGuid.ToString(),
                RevCodigo = model.RevCodigo,
                ClienteNombre = string.IsNullOrWhiteSpace(clienteNombre) ? model.ClienteNombre : clienteNombre,
                AtraccionNombre = string.IsNullOrWhiteSpace(atraccionNombre) ? model.AtraccionNombre : atraccionNombre,
                HorFecha = model.HorFecha?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                HorHoraInicio = model.HorHoraInicio?.ToString("HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty,
                RevTotal = model.RevTotal,
                RevEstado = model.RevEstado,
                FechaReserva = model.RevFechaReservaUtc,
                Detalle = model.Detalle.Select(d => new ReservaDetalleAdminResponse
                {
                    TipoParticipante = d.TckTipoParticipante,
                    Cantidad = d.RdetCantidad,
                    PrecioUnit = d.RdetPrecioUnit,
                    Subtotal = d.RdetSubtotal
                }).ToList()
            };
    }
}
