using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microservicio.Atracciones.Api.Models.Settings;
using Microservicio.Atracciones.Business.Interfaces.Integration;
using Microsoft.Extensions.Options;

namespace Microservicio.Atracciones.Api.Services;

public sealed class ClienteCrmSyncPublisher : IClienteCrmSyncPublisher
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http;
    private readonly ClientesSyncSettings _settings;
    private readonly ILogger<ClienteCrmSyncPublisher> _logger;

    public ClienteCrmSyncPublisher(
        HttpClient http,
        IOptions<ClientesSyncSettings> settings,
        ILogger<ClienteCrmSyncPublisher> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task EspejarAsync(ClienteCrmEspejo espejo, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            _logger.LogWarning(
                "ms-clientes CRM deshabilitado o sin BaseUrl: no se espeja cliente {UsuGuid}",
                espejo.UsuGuid);
            return;
        }

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "internal/v1/clientes/mirror");
            req.Headers.TryAddWithoutValidation("X-Monolith-Sync-Key", _settings.SyncApiKey);
            var payload = new MirrorPayload
            {
                UsuGuid = espejo.UsuGuid,
                TipoIdentificacion = espejo.TipoIdentificacion,
                NumeroIdentificacion = espejo.NumeroIdentificacion,
                Nombres = espejo.Nombres,
                Apellidos = espejo.Apellidos,
                RazonSocial = espejo.RazonSocial,
                Correo = espejo.Correo,
                Telefono = espejo.Telefono,
                Direccion = espejo.Direccion,
                CreadoPor = espejo.CreadoPor,
                IpCreador = espejo.IpCreador,
            };
            req.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOpts), Encoding.UTF8, "application/json");

            var res = await _http.SendAsync(req, cancellationToken);
            var body = await res.Content.ReadAsStringAsync(cancellationToken);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "CRM mirror (ms-clientes) falló {Status}: {Body}",
                    (int)res.StatusCode,
                    body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error llamando a ms-clientes CRM para usuario {UsuGuid}", espejo.UsuGuid);
        }
    }

    private sealed class MirrorPayload
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
}
