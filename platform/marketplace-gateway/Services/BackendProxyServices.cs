using System.Net.Http.Json;
using System.Text.Json;
using Atracciones.MarketplaceGateway.Options;
using Microsoft.Extensions.Options;

namespace Atracciones.MarketplaceGateway.Services;

public sealed class AtraccionesProxyService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public AtraccionesProxyService(HttpClient http, IOptions<ServicesOptions> options)
    {
        _http = http;
        _http.BaseAddress = new Uri(options.Value.AtraccionesHttp.TrimEnd('/') + "/");
    }

    public async Task<JsonElement> GetAtraccionesAsync(
        string? ciudad,
        string? tipo,
        string? subtipo,
        string? idioma,
        double? calificacionMin,
        bool? disponible,
        string? ordenarPor,
        int page,
        int limit,
        CancellationToken ct)
    {
        var qs = new List<string> { $"page={page}", $"limit={limit}" };
        if (!string.IsNullOrWhiteSpace(ciudad)) qs.Add($"ciudad={Uri.EscapeDataString(ciudad)}");
        if (!string.IsNullOrWhiteSpace(tipo)) qs.Add($"tipo={Uri.EscapeDataString(tipo)}");
        if (!string.IsNullOrWhiteSpace(subtipo)) qs.Add($"subtipo={Uri.EscapeDataString(subtipo)}");
        if (!string.IsNullOrWhiteSpace(idioma)) qs.Add($"idioma={Uri.EscapeDataString(idioma)}");
        if (calificacionMin.HasValue) qs.Add($"calificacion_min={calificacionMin.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        if (disponible.HasValue) qs.Add($"disponible={disponible.Value.ToString().ToLowerInvariant()}");
        if (!string.IsNullOrWhiteSpace(ordenarPor)) qs.Add($"ordenar_por={Uri.EscapeDataString(ordenarPor)}");

        var doc = await _http.GetFromJsonAsync<JsonElement>($"api/v2/atracciones?{string.Join('&', qs)}", JsonOpts, ct);
        return doc;
    }

    public Task<JsonElement> GetFiltrosAsync(string? ciudad, CancellationToken ct)
    {
        var url = string.IsNullOrWhiteSpace(ciudad)
            ? "api/v2/atracciones/filtros"
            : $"api/v2/atracciones/filtros?ciudad={Uri.EscapeDataString(ciudad)}";
        return _http.GetFromJsonAsync<JsonElement>(url, JsonOpts, ct)!;
    }

    public Task<JsonElement> GetAtraccionAsync(Guid guid, CancellationToken ct) =>
        _http.GetFromJsonAsync<JsonElement>($"api/v2/atracciones/{guid:D}", JsonOpts, ct)!;

    public Task<JsonElement> GetHorariosAsync(Guid atGuid, bool disponibles, CancellationToken ct) =>
        _http.GetFromJsonAsync<JsonElement>(
            $"api/v2/atracciones/{atGuid:D}/horarios?disponibles={disponibles.ToString().ToLowerInvariant()}",
            JsonOpts,
            ct)!;

    public Task<JsonElement> GetTicketsAsync(Guid atGuid, CancellationToken ct) =>
        _http.GetFromJsonAsync<JsonElement>($"api/v2/atracciones/{atGuid:D}/tickets", JsonOpts, ct)!;
}

public sealed class ReservasProxyService
{
    private readonly HttpClient _http;

    public ReservasProxyService(HttpClient http, IOptions<ServicesOptions> options)
    {
        _http = http;
        _http.BaseAddress = new Uri(options.Value.ReservasHttp.TrimEnd('/') + "/");
    }

    public async Task<JsonElement?> GetEstadoReservaAsync(Guid seguimientoId, CancellationToken ct)
    {
        using var resp = await _http.GetAsync($"internal/v1/marketplace/reservas/{seguimientoId:D}/estado", ct);
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
    }
}
