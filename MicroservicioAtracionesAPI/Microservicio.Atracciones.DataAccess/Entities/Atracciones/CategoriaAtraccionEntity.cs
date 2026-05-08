using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Atracciones
{
    public class CategoriaAtraccionEntity
    {
        public int CatId { get; set; }
        public int AtId { get; set; }

        // Auditoría
        public DateTime CaFechaIngreso { get; set; }
        public string CaUsuarioIngreso { get; set; } = string.Empty;
        public DateTime? CaFechaEliminacion { get; set; }
        public string? CaUsuarioEliminacion { get; set; }
        public char CaEstado { get; set; } = 'A';

        // Navegación
        public CategoriaEntity Categoria { get; set; } = null!;
        public AtraccionEntity Atraccion { get; set; } = null!;
    }
}
