using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Reservas
{
    public class TicketEntity
    {
        public int TckId { get; set; }
        public Guid TckGuid { get; set; }
        public int AtId { get; set; }
        public string TckTitulo { get; set; } = string.Empty;
        public decimal TckPrecio { get; set; }
        public string TckTipoParticipante { get; set; } = "Adulto";  // Adulto | Niño | Grupo | Estudiante | Senior
        public int TckCapacidadMaxima { get; set; }
        public int TckCuposDisponibles { get; set; }

        // Auditoría ingreso
        public DateTime TckFechaIngreso { get; set; }
        public string TckUsuarioIngreso { get; set; } = string.Empty;
        public string TckIpIngreso { get; set; } = string.Empty;

        // Auditoría modificación
        public DateTime? TckFechaMod { get; set; }
        public string? TckUsuarioMod { get; set; }
        public string? TckIpMod { get; set; }

        // Auditoría eliminación lógica
        public DateTime? TckFechaEliminacion { get; set; }
        public string? TckUsuarioEliminacion { get; set; }
        public string? TckIpEliminacion { get; set; }

        public char TckEstado { get; set; } = 'A';

        // Navegación
        public AtraccionEntity Atraccion { get; set; } = null!;
        public ICollection<HorarioEntity> Horarios { get; set; } = new List<HorarioEntity>();
        public ICollection<ReservaDetalleEntity> ReservasDetalle { get; set; } = new List<ReservaDetalleEntity>();
    }
}
