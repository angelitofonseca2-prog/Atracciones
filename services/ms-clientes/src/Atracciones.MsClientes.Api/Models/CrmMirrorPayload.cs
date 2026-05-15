using System.Text.Json.Serialization;

namespace Atracciones.MsClientes.Api.Models;

public sealed class CrmMirrorPayload
{
    [JsonPropertyName("usu_guid")]
    public Guid UsuGuid { get; set; }

    [JsonPropertyName("tipo_identificacion")]
    public string TipoIdentificacion { get; set; } = string.Empty;

    [JsonPropertyName("numero_identificacion")]
    public string NumeroIdentificacion { get; set; } = string.Empty;

    [JsonPropertyName("nombres")]
    public string? Nombres { get; set; }

    [JsonPropertyName("apellidos")]
    public string? Apellidos { get; set; }

    [JsonPropertyName("razon_social")]
    public string? RazonSocial { get; set; }

    [JsonPropertyName("correo")]
    public string Correo { get; set; } = string.Empty;

    [JsonPropertyName("telefono")]
    public string? Telefono { get; set; }

    [JsonPropertyName("direccion")]
    public string? Direccion { get; set; }

    [JsonPropertyName("creado_por")]
    public string CreadoPor { get; set; } = string.Empty;

    [JsonPropertyName("ip_creador")]
    public string IpCreador { get; set; } = string.Empty;
}
