using System.Text.Json.Serialization;

namespace Atracciones.MsIdentidad.Api.Models;

public sealed class MirrorRequest
{
    [JsonPropertyName("usu_id")]
    public int UsuId { get; set; }

    [JsonPropertyName("usu_guid")]
    public Guid UsuGuid { get; set; }

    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [JsonPropertyName("cli_id")]
    public int? CliId { get; set; }

    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = new();
}
