using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Public.Atracciones
{
    public class HorarioProximoResponse
    {
        public string HorGuid { get; set; } = string.Empty;
        public string TckGuid { get; set; } = string.Empty;
        public string TicketTitulo { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;   // "YYYY-MM-DD"
        public string HoraInicio { get; set; } = string.Empty;   // "HH:mm"
        public string? HoraFin { get; set; }                   // "HH:mm" | null
        public int Cupos { get; set; }
        public bool Disponible { get; set; }
    }
}
