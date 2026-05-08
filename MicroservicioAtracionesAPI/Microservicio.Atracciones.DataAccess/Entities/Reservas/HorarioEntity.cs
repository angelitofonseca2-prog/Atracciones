using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Reservas
{
    public class HorarioEntity
    {
        public int HorId { get; set; }
        public Guid HorGuid { get; set; }
        public int TckId { get; set; }
        public DateOnly HorFecha { get; set; }
        public TimeOnly HorHoraInicio { get; set; }
        public TimeOnly? HorHoraFin { get; set; }
        public int HorCuposDisponibles { get; set; }

        // Auditoría ingreso
        public DateTime HorFechaIngreso { get; set; }
        public string HorUsuarioIngreso { get; set; } = string.Empty;
        public string HorIpIngreso { get; set; } = string.Empty;

        // Auditoría modificación
        public DateTime? HorFechaMod { get; set; }
        public string? HorUsuarioMod { get; set; }
        public string? HorIpMod { get; set; }

        // Auditoría eliminación lógica
        public DateTime? HorFechaEliminacion { get; set; }
        public string? HorUsuarioEliminacion { get; set; }
        public string? HorIpEliminacion { get; set; }

        public char HorEstado { get; set; } = 'A';

        // Navegación
        public TicketEntity Ticket { get; set; } = null!;
        public ICollection<ReservaEntity> Reservas { get; set; } = new List<ReservaEntity>();
    }
}
