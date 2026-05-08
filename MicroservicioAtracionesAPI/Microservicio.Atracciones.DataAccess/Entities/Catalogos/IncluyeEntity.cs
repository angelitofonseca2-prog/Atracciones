using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Catalogos
{
    public class IncluyeEntity
    {
        public int IncId { get; set; }
        public Guid IncGuid { get; set; }
        public string IncDescripcion { get; set; } = string.Empty;
        public char IncEstado { get; set; } = 'A';

        // Navegación
        public ICollection<AtraccionIncluyeEntity> AtraccionesIncluyen { get; set; } = new List<AtraccionIncluyeEntity>();
    }
}
