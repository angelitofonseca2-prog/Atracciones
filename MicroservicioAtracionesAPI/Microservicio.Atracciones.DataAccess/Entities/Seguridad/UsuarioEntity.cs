using Microservicio.Atracciones.DataAccess.Entities.Clientes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Seguridad
{
    public class UsuarioEntity
    {
        public int UsuId { get; set; }
        public Guid UsuGuid { get; set; }
        public string UsuLogin { get; set; } = string.Empty;
        public string UsuPasswordHash { get; set; } = string.Empty;

        // Auditoría registro
        public DateTime UsuFechaRegistro { get; set; }
        public string UsuUsuarioRegistro { get; set; } = string.Empty;
        public string UsuIpRegistro { get; set; } = string.Empty;

        // Auditoría modificación
        public DateTime? UsuFechaMod { get; set; }
        public string? UsuUsuarioMod { get; set; }
        public string? UsuIpMod { get; set; }

        // Auditoría eliminación lógica
        public DateTime? UsuFechaEliminacion { get; set; }
        public string? UsuUsuarioEliminacion { get; set; }
        public string? UsuIpEliminacion { get; set; }

        // Estado: 'A' = Activo, 'I' = Inactivo
        public char UsuEstado { get; set; } = 'A';

        // ----------------------------------------------------------------
        //  Navegación
        // ----------------------------------------------------------------
        public ICollection<UsuarioRolEntity> UsuariosRoles { get; set; } = new List<UsuarioRolEntity>();
        public ClienteEntity? Cliente { get; set; }
    }

}
