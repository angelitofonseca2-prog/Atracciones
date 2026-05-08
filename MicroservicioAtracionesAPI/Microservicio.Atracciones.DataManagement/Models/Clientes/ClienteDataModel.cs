namespace Microservicio.Atracciones.DataManagement.Models.Clientes;

/// <summary>
/// Modelo de datos de cliente para la capa Business.
/// Aplana la relación con USUARIO: solo expone los datos
/// necesarios para operar sobre clientes.
/// </summary>
public class ClienteDataModel
{
    public int CliId { get; set; }
    public Guid CliGuid { get; set; }
    public int? UsuId { get; set; }

    // Identificación
    public string CliTipoIdentificacion { get; set; } = string.Empty;
    public string CliNumeroIdentificacion { get; set; } = string.Empty;

    // Persona natural
    public string? CliNombres { get; set; }
    public string? CliApellidos { get; set; }

    // Empresa
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

    public char CliEstado { get; set; }
}
