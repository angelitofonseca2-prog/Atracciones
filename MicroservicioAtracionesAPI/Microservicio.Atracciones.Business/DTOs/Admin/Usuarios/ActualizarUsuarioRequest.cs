using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Usuarios
{
    public class ActualizarUsuarioRequest
    {
        [MaxLength(256)] public string? NuevaPassword { get; set; }
        public IList<string>? Roles { get; set; }
    }

}
