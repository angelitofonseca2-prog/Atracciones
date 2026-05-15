using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microservicio.Atracciones.Api.Models.Settings;
using Microservicio.Atracciones.Business.Interfaces.Integration;
using Microsoft.Extensions.Options;

namespace Microservicio.Atracciones.Api.Services;

public sealed class IdentidadUsuarioSyncPublisher : IIdentidadUsuarioSyncPublisher
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http;
    private readonly IdentidadSyncSettings _settings;
    private readonly ILogger<IdentidadUsuarioSyncPublisher> _logger;

    public IdentidadUsuarioSyncPublisher(
        HttpClient http,
        IOptions<IdentidadSyncSettings> settings,
        ILogger<IdentidadUsuarioSyncPublisher> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<IdentidadTokenResult?> SincronizarYObtenerTokenAsync(
        IdentidadUsuarioEspejo espejo,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            _logger.LogWarning("ms-identidad deshabilitado o sin BaseUrl: no se sincroniza usuario {UsuId}", espejo.UsuId);
            return null;
        }

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "internal/v1/auth/mirror");
            req.Headers.TryAddWithoutValidation("X-Monolith-Sync-Key", _settings.SyncApiKey);
            var payload = new MirrorPayload
            {
                UsuId = espejo.UsuId,
                UsuGuid = espejo.UsuGuid,
                Login = espejo.Login,
                PasswordHash = espejo.PasswordHash,
                CliId = espejo.CliId,
                Roles = espejo.Roles.ToList(),
            };
            var json = JsonSerializer.Serialize(payload, JsonOpts);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _http.SendAsync(req, cancellationToken);
            var body = await res.Content.ReadAsStringAsync(cancellationToken);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Identidad mirror falló {Status}: {Body}",
                    (int)res.StatusCode,
                    body);
                return null;
            }

            var envelope = JsonSerializer.Deserialize<IdentidadMirrorEnvelope>(body, JsonOpts);
            if (envelope?.Data is null || string.IsNullOrWhiteSpace(envelope.Data.Token))
                return null;

            return new IdentidadTokenResult(
                envelope.Data.Token,
                envelope.Data.Expiracion,
                envelope.Data.Login,
                envelope.Data.Roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error llamando a ms-identidad para usuario {UsuId}", espejo.UsuId);
            return null;
        }
    }

    private sealed class MirrorPayload
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

    private sealed class IdentidadMirrorEnvelope
    {
        public IdentidadMirrorData? Data { get; set; }
    }

    private sealed class IdentidadMirrorData
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiracion { get; set; }
        public string Login { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
