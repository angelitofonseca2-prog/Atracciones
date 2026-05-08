using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Public.Atracciones
{
    public class TicketDisponibleResponse
    {
        public string TckGuid { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string Moneda { get; set; } = "USD";
        public int CuposDisponibles { get; set; }
        public IList<HorarioProximoResponse> HorariosDisponibles { get; set; } = new List<HorarioProximoResponse>();
    }
}
