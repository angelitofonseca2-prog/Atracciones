using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Atracciones
{
    public class ImagenAtraccionEntity
    {
        public int ImgId { get; set; }
        public int AtId { get; set; }

        // Auditoría
        public DateTime ImaFechaIngreso { get; set; }
        public string ImaUsuarioIngreso { get; set; } = string.Empty;
        public DateTime? ImaFechaEliminacion { get; set; }
        public string? ImaUsuarioEliminacion { get; set; }
        public char ImaEstado { get; set; } = 'A';

        // Navegación
        public ImagenEntity Imagen { get; set; } = null!;
        public AtraccionEntity Atraccion { get; set; } = null!;
    }
}
