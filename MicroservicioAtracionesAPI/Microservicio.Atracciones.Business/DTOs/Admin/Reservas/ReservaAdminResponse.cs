using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Reservas
{
    public class ReservaAdminResponse
    {
        public string RevGuid { get; set; } = string.Empty;
        public string RevCodigo { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string AtraccionNombre { get; set; } = string.Empty;
        public string HorFecha { get; set; } = string.Empty;
        public string HorHoraInicio { get; set; } = string.Empty;
        public decimal RevTotal { get; set; }
        public char RevEstado { get; set; }
        public DateTime FechaReserva { get; set; }
        public IList<ReservaDetalleAdminResponse> Detalle { get; set; } = new List<ReservaDetalleAdminResponse>();
    }
}
