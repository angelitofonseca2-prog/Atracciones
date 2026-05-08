using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Catalogos
{
    public class ImagenEntity
    {
        public int ImgId { get; set; }
        public Guid ImgGuid { get; set; }
        public string ImgUrl { get; set; } = string.Empty;
        public string? ImgDescripcion { get; set; }

        // Auditoría ingreso
        public DateTime ImgFechaIngreso { get; set; }
        public string ImgUsuarioIngreso { get; set; } = string.Empty;
        public string ImgIpIngreso { get; set; } = string.Empty;

        // Auditoría modificación
        public DateTime? ImgFechaMod { get; set; }
        public string? ImgUsuarioMod { get; set; }
        public string? ImgIpMod { get; set; }

        // Auditoría eliminación lógica
        public DateTime? ImgFechaEliminacion { get; set; }
        public string? ImgUsuarioEliminacion { get; set; }
        public string? ImgIpEliminacion { get; set; }

        public char ImgEstado { get; set; } = 'A';

        // Navegación
        public ICollection<ImagenAtraccionEntity> ImagenesAtracciones { get; set; } = new List<ImagenAtraccionEntity>();
    }
}
