namespace Atracciones.MsClientes.DataAccess.Entities;

public sealed class ClienteEntity
{
    /// <summary>PK = usu_guid (alineado con ms-identidad).</summary>
    public Guid CliGuid { get; set; }

    public string CliTipoIdentificacion { get; set; } = string.Empty;
    public string CliNumeroIdentificacion { get; set; } = string.Empty;
    public string? CliNombres { get; set; }
    public string? CliApellidos { get; set; }
    public string? CliRazonSocial { get; set; }
    public string CliCorreo { get; set; } = string.Empty;
    public string? CliTelefono { get; set; }
    public string? CliDireccion { get; set; }
    public char CliEstado { get; set; } = 'A';
    public DateTime CliFechaIngreso { get; set; }
    public string CliUsuarioIngreso { get; set; } = string.Empty;
    public string CliIpIngreso { get; set; } = string.Empty;
}
