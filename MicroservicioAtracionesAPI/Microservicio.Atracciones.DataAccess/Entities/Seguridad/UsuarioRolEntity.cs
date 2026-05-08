using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Seguridad
{
    public class UsuarioRolEntity
    {
        public int UsuRolId { get; set; }
        public int UsuId { get; set; }
        public int RolId { get; set; }
        public char UsuRolEstado { get; set; } = 'A';

        // ----------------------------------------------------------------
        //  Navegación
        // ----------------------------------------------------------------
        public UsuarioEntity UsuEntity { get; set; } = null!;
        public RolEntity RolEntity { get; set; } = null!;
    }

}
