using System.Text;
using System.Text.Json;
using Atracciones.MsCatalogos.Business;
using Atracciones.MsCatalogos.Business.Integration;
using Microsoft.Extensions.Options;

namespace Atracciones.MsCatalogos.Api.Integration;

public sealed class MonolithCatalogLegacyPublisher : IMonolithCatalogLegacyPublisher
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http;
    private readonly MonolithCatalogLegacySyncOptions _opts;
    private readonly ILogger<MonolithCatalogLegacyPublisher> _logger;

    public MonolithCatalogLegacyPublisher(
        HttpClient http,
        IOptions<MonolithCatalogLegacySyncOptions> opts,
        ILogger<MonolithCatalogLegacyPublisher> logger)
    {
        _http = http;
        _opts = opts.Value;
        _logger = logger;
    }

    public async Task PublishAsync(CatalogMirrorBatch batch, CancellationToken cancellationToken = default)
    {
        if (!_opts.Enabled || string.IsNullOrWhiteSpace(_opts.BaseUrl))
        {
            _logger.LogDebug("Mirror catálogo legacy deshabilitado o sin BaseUrl; no se publica.");
            return;
        }

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "internal/v1/catalogos/mirror");
            req.Headers.TryAddWithoutValidation("X-Monolith-Sync-Key", _opts.SyncApiKey);
            req.Content = new StringContent(JsonSerializer.Serialize(batch, JsonOpts), Encoding.UTF8, "application/json");

            var res = await _http.SendAsync(req, cancellationToken);
            var body = await res.Content.ReadAsStringAsync(cancellationToken);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Mirror catálogo legacy falló {Status}: {Body}",
                    (int)res.StatusCode,
                    body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error llamando mirror catálogo legacy al monolito.");
        }
    }
}
