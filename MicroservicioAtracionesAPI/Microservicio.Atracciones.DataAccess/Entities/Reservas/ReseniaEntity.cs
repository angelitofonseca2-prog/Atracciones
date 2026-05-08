using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Reservas
{
    public class ReseniaEntity
    {
        public int RsnId { get; set; }
        public Guid RsnGuid { get; set; }
        public int AtId { get; set; }
        public int RevId { get; set; }  // FK → RESERVAS
        public string? RsnComentario { get; set; }
        public short RsnRating { get; set; }  // 1 a 5

        // Auditoría creación
        public DateTime RsnFechaCreacion { get; set; }
        public string RsnUsuarioCreacion { get; set; } = string.Empty;
        public string RsnIpCreacion { get; set; } = string.Empty;

        // Auditoría modificación
        public DateTime? RsnFechaMod { get; set; }
        public string? RsnUsuarioMod { get; set; }
        public string? RsnIpMod { get; set; }

        // Auditoría eliminación lógica
        public DateTime? RsnFechaEliminacion { get; set; }
        public string? RsnUsuarioEliminacion { get; set; }
        public string? RsnIpEliminacion { get; set; }

        public char RsnEstado { get; set; } = 'A';

        // Navegación
        public AtraccionEntity Atraccion { get; set; } = null!;
        public ReservaEntity Reserva { get; set; } = null!;
    }
}
