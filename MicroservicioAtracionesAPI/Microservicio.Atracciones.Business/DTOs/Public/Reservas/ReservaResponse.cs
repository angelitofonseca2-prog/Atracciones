using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Public.Reservas
{
    public class ReservaResponse
    {
        public string RevGuid { get; set; } = string.Empty;
        public string RevCodigo { get; set; } = string.Empty;
        public string HorFecha { get; set; } = string.Empty;
        public string HorHoraInicio { get; set; } = string.Empty;
        public string? HorHoraFin { get; set; }
        public string AtraccionNombre { get; set; } = string.Empty;
        public decimal RevSubtotal { get; set; }
        public decimal RevValorIva { get; set; }
        public decimal RevTotal { get; set; }
        public string Moneda { get; set; } = "USD";
        public string RevEstado { get; set; } = string.Empty;
        public DateTime RevFechaReservaUtc { get; set; }

        public IList<ReservaDetalleResponse> Detalle { get; set; } = new List<ReservaDetalleResponse>();
        public Dictionary<string, string?> Links { get; set; } = new();
    }
}
