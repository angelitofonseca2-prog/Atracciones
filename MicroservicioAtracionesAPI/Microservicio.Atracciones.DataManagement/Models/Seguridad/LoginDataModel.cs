using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Models.Seguridad
{
    public class LoginDataModel
    {
        public int UsuId { get; set; }
        public Guid UsuGuid { get; set; }
        public string UsuLogin { get; set; } = string.Empty;
        public string UsuPasswordHash { get; set; } = string.Empty;
        public char UsuEstado { get; set; }
        public int? CliId { get; set; } // E-05: Si el usuario es un cliente, aquí va su ID de cliente
        public IReadOnlyList<string> Roles { get; set; } = new List<string>();
    }

}
