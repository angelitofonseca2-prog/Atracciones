using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Models.Seguridad
{
    public class RolDataModel
    {
        public int RolId { get; set; }
        public Guid RolGuid { get; set; }
        public string RolDescripcion { get; set; } = string.Empty;
        public char RolEstado { get; set; }
    }
}
