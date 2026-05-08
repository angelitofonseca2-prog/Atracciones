using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Auditoria
{
    public class AuditoriaLogEntity
    {
        public long LogId { get; set; }
        public Guid LogGuid { get; set; }

        // Referencia lógica (sin FK física)
        public string LogTabla { get; set; } = string.Empty;
        public string LogOperacion { get; set; } = string.Empty;  // INSERT | UPDATE | DELETE
        public int? LogRegistroId { get; set; }
        public Guid? LogRegistroGuid { get; set; }

        // Snapshots JSON
        public string? LogDatosAnteriores { get; set; }  // null en INSERT
        public string? LogDatosNuevos { get; set; }  // null en DELETE

        // Trazabilidad
        public DateTime LogFechaUtc { get; set; }
        public string LogUsuario { get; set; } = string.Empty;
        public string LogIp { get; set; } = string.Empty;
        public string? LogOrigenCanal { get; set; }
    }
}
