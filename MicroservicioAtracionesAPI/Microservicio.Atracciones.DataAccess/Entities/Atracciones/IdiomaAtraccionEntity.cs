using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Atracciones
{
    public class IdiomaAtraccionEntity
    {
        public int IdId { get; set; }
        public int AtId { get; set; }

        // Auditoría
        public DateTime IaFechaIngreso { get; set; }
        public string IaUsuarioIngreso { get; set; } = string.Empty;
        public DateTime? IaFechaEliminacion { get; set; }
        public string? IaUsuarioEliminacion { get; set; }
        public char IaEstado { get; set; } = 'A';

        // Navegación
        public IdiomaEntity Idioma { get; set; } = null!;
        public AtraccionEntity Atraccion { get; set; } = null!;
    }

}
