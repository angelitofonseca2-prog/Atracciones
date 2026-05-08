using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Models.Seguridad
{
    public class UsuarioDataModel
    {
        public int UsuId { get; set; }
        public Guid UsuGuid { get; set; }
        public string UsuLogin { get; set; } = string.Empty;
        public string UsuPasswordHash { get; set; } = string.Empty;
        public char UsuEstado { get; set; }
        public DateTime UsuFechaRegistro { get; set; }

        // Auditoría ingreso
        public string UsuUsuarioRegistro { get; set; } = string.Empty;
        public string UsuIpRegistro { get; set; } = string.Empty;

        // Auditoría modificación
        public DateTime? UsuFechaMod { get; set; }
        public string? UsuUsuarioMod { get; set; }
        public string? UsuIpMod { get; set; }

        public int? CliId { get; set; } // E-05

        // Roles asociados (navegación plana)
        public IReadOnlyList<RolDataModel> Roles { get; set; } = new List<RolDataModel>();
    }
}
