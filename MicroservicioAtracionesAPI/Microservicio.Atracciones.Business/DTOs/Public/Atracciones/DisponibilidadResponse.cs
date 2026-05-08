using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Public.Atracciones
{
    public class DisponibilidadResponse
    {
        public bool Disponible { get; set; }
        public bool DisponibleHoy { get; set; }
        public string? ProximaFechaDisponible { get; set; }  // ISO date | null
        public int? CuposDisponibles { get; set; }
    }
}
