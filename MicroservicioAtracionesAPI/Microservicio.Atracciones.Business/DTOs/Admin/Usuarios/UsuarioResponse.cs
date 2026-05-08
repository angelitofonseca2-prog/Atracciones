using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Usuarios
{
    public class UsuarioResponse
    {
        public string UsuGuid { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public char Estado { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public DateTime FechaRegistro { get; set; }
    }
}
