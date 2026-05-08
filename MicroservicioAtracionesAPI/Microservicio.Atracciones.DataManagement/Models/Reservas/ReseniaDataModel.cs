using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Models.Reservas
{
    public class ReseniaDataModel
    {
        public int RsnId { get; set; }
        public Guid RsnGuid { get; set; }
        public int AtId { get; set; }
        public int RevId { get; set; }
        public string? RsnComentario { get; set; }
        public short RsnRating { get; set; }
        public char RsnEstado { get; set; }

        // Auditoría
        public DateTime RsnFechaCreacion { get; set; }
        public string RsnUsuarioCreacion { get; set; } = string.Empty;
        public string RsnIpCreacion { get; set; } = string.Empty;
        public DateTime? RsnFechaMod { get; set; }
        public string? RsnUsuarioMod { get; set; }
        public string? RsnIpMod { get; set; }
        public DateTime? RsnFechaEliminacion { get; set; }
        public string? RsnUsuarioEliminacion { get; set; }
        public string? RsnIpEliminacion { get; set; }
    }
}
