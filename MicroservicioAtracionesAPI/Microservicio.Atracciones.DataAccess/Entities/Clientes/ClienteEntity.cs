using Microservicio.Atracciones.DataAccess.Entities.Reservas;
using Microservicio.Atracciones.DataAccess.Entities.Seguridad;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Clientes
{
    public class ClienteEntity
    {
        public int CliId { get; set; }
        public Guid CliGuid { get; set; }
        public int? UsuId { get; set; }

        // Datos de identificación
        public string CliTipoIdentificacion { get; set; } = string.Empty;   // CC | RUC | PASAPORTE | CEDULA | OTRO
        public string CliNumeroIdentificacion { get; set; } = string.Empty;

        // Datos persona natural
        public string? CliNombres { get; set; }
        public string? CliApellidos { get; set; }

        // Datos empresa
        public string? CliRazonSocial { get; set; }

        // Contacto
        public string CliCorreo { get; set; } = string.Empty;
        public string? CliTelefono { get; set; }
        public string? CliDireccion { get; set; }

        // Auditoría ingreso
        public DateTime CliFechaIngreso { get; set; }
        public string CliUsuarioIngreso { get; set; } = string.Empty;
        public string CliIpIngreso { get; set; } = string.Empty;

        // Auditoría eliminación lógica
        public DateTime? CliFechaEliminacion { get; set; }
        public string? CliUsuarioEliminacion { get; set; }
        public string? CliIpEliminacion { get; set; }

        // Estado: 'A' = Activo, 'I' = Inactivo
        public char CliEstado { get; set; } = 'A';

        // ----------------------------------------------------------------
        //  Navegación
        // ----------------------------------------------------------------
        public UsuarioEntity? Usuario { get; set; }
        public ICollection<ReservaEntity> Reservas { get; set; } = new List<ReservaEntity>();
    }
}
