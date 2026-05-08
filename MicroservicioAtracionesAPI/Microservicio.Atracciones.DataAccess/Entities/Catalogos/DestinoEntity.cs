using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Catalogos
{
    public class DestinoEntity
    {
        public int DesId { get; set; }
        public Guid DesGuid { get; set; }
        public string DesNombre { get; set; } = string.Empty;
        public string DesPais { get; set; } = string.Empty;
        public string? DesImagenUrl { get; set; }

        // Auditoría ingreso
        public DateTime DesFechaIngreso { get; set; }
        public string DesUsuarioIngreso { get; set; } = string.Empty;
        public string DesIpIngreso { get; set; } = string.Empty;

        // Auditoría modificación
        public DateTime? DesFechaMod { get; set; }
        public string? DesUsuarioMod { get; set; }
        public string? DesIpMod { get; set; }

        // Auditoría eliminación lógica
        public DateTime? DesFechaEliminacion { get; set; }
        public string? DesUsuarioEliminacion { get; set; }
        public string? DesIpEliminacion { get; set; }

        public char DesEstado { get; set; } = 'A';

        // Navegación
        public ICollection<AtraccionEntity> Atracciones { get; set; } = new List<AtraccionEntity>();
    }
}
