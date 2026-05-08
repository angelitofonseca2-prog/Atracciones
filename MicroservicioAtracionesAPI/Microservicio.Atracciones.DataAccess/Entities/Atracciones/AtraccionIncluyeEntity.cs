using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Atracciones
{
    public class AtraccionIncluyeEntity
    {
        public int IncId { get; set; }
        public int AtId { get; set; }

        // Auditoría
        public DateTime AiFechaIngreso { get; set; }
        public string AiUsuarioIngreso { get; set; } = string.Empty;
        public DateTime? AiFechaEliminacion { get; set; }
        public string? AiUsuarioEliminacion { get; set; }
        public char AiEstado { get; set; } = 'A';

        // Navegación
        public IncluyeEntity Incluye { get; set; } = null!;
        public AtraccionEntity Atraccion { get; set; } = null!;
    }
}
