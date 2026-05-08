using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Catalogos
{
    public class CategoriaEntity
    {
        public int CatId { get; set; }
        public Guid CatGuid { get; set; }
        public int? CatParentId { get; set; }  // null = categoría raíz
        public string CatNombre { get; set; } = string.Empty;

        // Auditoría ingreso
        public DateTime CatFechaIngreso { get; set; }
        public string CatUsuarioIngreso { get; set; } = string.Empty;
        public string CatIpIngreso { get; set; } = string.Empty;

        // Auditoría modificación
        public DateTime? CatFechaMod { get; set; }
        public string? CatUsuarioMod { get; set; }
        public string? CatIpMod { get; set; }

        // Auditoría eliminación lógica
        public DateTime? CatFechaEliminacion { get; set; }
        public string? CatUsuarioEliminacion { get; set; }
        public string? CatIpEliminacion { get; set; }

        public char CatEstado { get; set; } = 'A';

        // Navegación (auto-referencia)
        public CategoriaEntity? Parent { get; set; }
        public ICollection<CategoriaEntity> Hijos { get; set; } = new List<CategoriaEntity>();
        public ICollection<CategoriaAtraccionEntity> CategoriasAtracciones { get; set; } = new List<CategoriaAtraccionEntity>();
    }
}
