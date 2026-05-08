using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Catalogos
{
    public class IdiomaEntity
    {
        public int IdId { get; set; }
        public Guid IdGuid { get; set; }
        public string IdDescripcion { get; set; } = string.Empty;

        // Auditoría ingreso
        public DateTime IdFechaIngreso { get; set; }
        public string IdUsuarioIngreso { get; set; } = string.Empty;
        public string IdIpIngreso { get; set; } = string.Empty;

        // Auditoría modificación
        public DateTime? IdFechaMod { get; set; }
        public string? IdUsuarioMod { get; set; }
        public string? IdIpMod { get; set; }

        // Auditoría eliminación lógica
        public DateTime? IdFechaEliminacion { get; set; }
        public string? IdUsuarioEliminacion { get; set; }
        public string? IdIpEliminacion { get; set; }

        public char IdEstado { get; set; } = 'A';

        // Navegación
        public ICollection<IdiomaAtraccionEntity> IdiomasAtracciones { get; set; } = new List<IdiomaAtraccionEntity>();
    }

}
