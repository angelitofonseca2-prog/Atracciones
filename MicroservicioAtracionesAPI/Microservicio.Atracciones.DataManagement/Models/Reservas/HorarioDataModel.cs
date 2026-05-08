using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Models.Reservas
{
    public class HorarioDataModel
    {
        public int HorId { get; set; }
        public Guid HorGuid { get; set; }
        public int TckId { get; set; }
        public Guid TckGuid { get; set; }
        public int AtId { get; set; }
        public Guid AtGuid { get; set; }
        public string AtNombre { get; set; } = string.Empty;
        public string TckTitulo { get; set; } = string.Empty;
        public int TckCapacidadMaxima { get; set; }
        public DateOnly HorFecha { get; set; }
        public TimeOnly HorHoraInicio { get; set; }
        public TimeOnly? HorHoraFin { get; set; }
        public int HorCuposDisponibles { get; set; }
        public char HorEstado { get; set; }

        // Auditoría
        public DateTime HorFechaIngreso { get; set; }
        public string HorUsuarioIngreso { get; set; } = string.Empty;
        public string HorIpIngreso { get; set; } = string.Empty;
        public DateTime? HorFechaMod { get; set; }
        public string? HorUsuarioMod { get; set; }
        public string? HorIpMod { get; set; }
        public DateTime? HorFechaEliminacion { get; set; }
        public string? HorUsuarioEliminacion { get; set; }
        public string? HorIpEliminacion { get; set; }
    }
}
