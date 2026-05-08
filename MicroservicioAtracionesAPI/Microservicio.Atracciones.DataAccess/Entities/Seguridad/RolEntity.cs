using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Seguridad
{
    public class RolEntity
    {
        public int RolId { get; set; }
        public Guid RolGuid { get; set; }
        public string RolDescripcion { get; set; } = string.Empty;

        // Auditoría ingreso
        public DateTime RolFechaIngreso { get; set; }
        public string RolUsuarioIngreso { get; set; } = string.Empty;
        public string RolIpIngreso { get; set; } = string.Empty;

        // Auditoría eliminación lógica
        public DateTime? RolFechaEliminacion { get; set; }
        public string? RolUsuarioEliminacion { get; set; }
        public string? RolIpEliminacion { get; set; }

        // Estado: 'A' = Activo, 'I' = Inactivo
        public char RolEstado { get; set; } = 'A';

        // ----------------------------------------------------------------
        //  Navegación
        // ----------------------------------------------------------------
        public ICollection<UsuarioRolEntity> UsuariosRoles { get; set; } = new List<UsuarioRolEntity>();
    }

}
