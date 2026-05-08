using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Models.Auditoria
{
    public class AuditoriaLogDataModel
    {
        public string LogTabla { get; set; } = string.Empty;
        public string LogOperacion { get; set; } = string.Empty;
        public int? LogRegistroId { get; set; }
        public Guid? LogRegistroGuid { get; set; }
        public string? LogDatosAnteriores { get; set; }
        public string? LogDatosNuevos { get; set; }
        public string LogUsuario { get; set; } = string.Empty;
        public string LogIp { get; set; } = string.Empty;
        public string? LogOrigenCanal { get; set; }
        public DateTime LogFechaUtc { get; set; } = DateTime.UtcNow;
    }
}
