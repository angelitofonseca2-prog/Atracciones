using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Atracciones.MsOrquestador.Business.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atracciones.MsOrquestador.Business.Integration;

public sealed class PayPalApiClient
{
    private const string ClientName = "paypal";
    private readonly IHttpClientFactory _httpFactory;
    private readonly IOptionsMonitor<PayPalOptions> _options;
    private readonly ILogger<PayPalApiClient> _logger;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _accessToken;
    private DateTimeOffset _accessTokenExpires = DateTimeOffset.MinValue;

    public PayPalApiClient(
        IHttpClientFactory httpFactory,
        IOptionsMonitor<PayPalOptions> options,
        ILogger<PayPalApiClient> logger)
    {
        _httpFactory = httpFactory;
        _options = options;
        _logger = logger;
    }

    public bool IsConfigured => _options.CurrentValue.IsConfigured;

    public async Task<string> CreateOrderAsync(
        decimal amount,
        string currencyCode,
        string revGuidD,
        CancellationToken ct)
    {
        var token = await GetAccessTokenAsync(ct);
        var client = CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Post, "v2/checkout/orders");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Headers.TryAddWithoutValidation("Prefer", "return=representation");
        var value = amount.ToString("F2", CultureInfo.InvariantCulture);
        static string J(string s) => s.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
        var body =
            "{\"intent\":\"CAPTURE\",\"purchase_units\":[{\"amount\":{\"currency_code\":\"" +
            J(currencyCode) +
            "\",\"value\":\"" + value +
            "\"},\"custom_id\":\"" + J(revGuidD) +
            "\"}]}";
        req.Content = new StringContent(body, Encoding.UTF8, "application/json");

        using var resp = await client.SendAsync(req, ct);
        var json = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("PayPal CreateOrder falló {Status}: {Body}", (int)resp.StatusCode, json);
            throw new InvalidOperationException("PayPal rechazó la creación de la orden.");
        }

        using var doc = JsonDocument.Parse(json);
        var id = doc.RootElement.GetProperty("id").GetString();
        if (string.IsNullOrEmpty(id))
            throw new InvalidOperationException("Respuesta PayPal sin id de orden.");
        return id;
    }

    public async Task<PayPalCaptureResult> CaptureOrderAsync(string paypalOrderId, CancellationToken ct)
    {
        var token = await GetAccessTokenAsync(ct);
        var client = CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Post, $"v2/checkout/orders/{Uri.EscapeDataString(paypalOrderId)}/capture");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        using var resp = await client.SendAsync(req, ct);
        var json = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("PayPal Capture falló {Status}: {Body}", (int)resp.StatusCode, json);
            throw new InvalidOperationException("PayPal rechazó la captura.");
        }

        return ParseCapture(json);
    }

    public async Task<bool> VerifyWebhookSignatureAsync(
        string transmissionId,
        string transmissionTime,
        string certUrl,
        string authAlgo,
        string transmissionSig,
        JsonElement webhookEvent,
        CancellationToken ct)
    {
        var opt = _options.CurrentValue;
        if (string.IsNullOrWhiteSpace(opt.WebhookId))
        {
            _logger.LogWarning("PayPal:WebhookId no configurado; se rechaza el webhook.");
            return false;
        }

        var token = await GetAccessTokenAsync(ct);
        var client = CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Post, "v1/notifications/verify-webhook-signature");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new Dictionary<string, object?>
        {
            ["transmission_id"] = transmissionId,
            ["transmission_time"] = transmissionTime,
            ["cert_url"] = certUrl,
            ["auth_algo"] = authAlgo,
            ["transmission_sig"] = transmissionSig,
            ["webhook_id"] = opt.WebhookId,
            ["webhook_event"] = webhookEvent,
        };
        var body = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        });
        req.Content = new StringContent(body, Encoding.UTF8, "application/json");

        using var resp = await client.SendAsync(req, ct);
        var outJson = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("PayPal verify-webhook-signature {Status}: {Body}", (int)resp.StatusCode, outJson);
            return false;
        }

        using var doc = JsonDocument.Parse(outJson);
        return doc.RootElement.TryGetProperty("verification_status", out var st)
            && string.Equals(st.GetString(), "SUCCESS", StringComparison.OrdinalIgnoreCase);
    }

    private HttpClient CreateClient()
    {
        var opt = _options.CurrentValue;
        var b = string.IsNullOrWhiteSpace(opt.BaseUrl)
            ? "https://api-m.sandbox.paypal.com"
            : opt.BaseUrl.TrimEnd('/');
        var client = _httpFactory.CreateClient(ClientName);
        client.BaseAddress = new Uri(b + "/");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        var opt = _options.CurrentValue;
        if (!opt.IsConfigured)
            throw new InvalidOperationException("PayPal no está configurado.");

        if (_accessToken is not null && DateTimeOffset.UtcNow < _accessTokenExpires)
            return _accessToken;

        await _tokenLock.WaitAsync(ct);
        try
        {
            if (_accessToken is not null && DateTimeOffset.UtcNow < _accessTokenExpires)
                return _accessToken;

            var client = CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, "v1/oauth2/token");
            var cred = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{opt.ClientId}:{opt.ClientSecret}"));
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", cred);
            req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
            });

            using var resp = await client.SendAsync(req, ct);
            var json = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("PayPal OAuth falló {Status}: {Body}", (int)resp.StatusCode, json);
                throw new InvalidOperationException("No se pudo autenticar con PayPal.");
            }

            using var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("access_token").GetString();
            var expiresIn = doc.RootElement.TryGetProperty("expires_in", out var ei) ? ei.GetInt32() : 300;
            if (string.IsNullOrEmpty(token))
                throw new InvalidOperationException("Token PayPal vacío.");

            _accessToken = token;
            _accessTokenExpires = DateTimeOffset.UtcNow.AddSeconds(Math.Max(60, expiresIn - 60));
            return token;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private static PayPalCaptureResult ParseCapture(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var status = root.TryGetProperty("status", out var st) ? st.GetString() : null;
        if (!string.Equals(status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Estado de orden tras captura no completado: {status}");

        if (!root.TryGetProperty("purchase_units", out var units) || units.GetArrayLength() == 0)
            throw new InvalidOperationException("Respuesta PayPal sin purchase_units.");

        var unit0 = units[0];
        if (!unit0.TryGetProperty("payments", out var payments)
            || !payments.TryGetProperty("captures", out var captures)
            || captures.GetArrayLength() == 0)
            throw new InvalidOperationException("Respuesta PayPal sin capturas.");

        var cap = captures[0];
        var captureId = cap.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("Captura sin id.");
        if (!cap.TryGetProperty("amount", out var amountEl))
            throw new InvalidOperationException("Captura sin amount.");
        var currency = amountEl.GetProperty("currency_code").GetString() ?? "USD";
        var valueStr = amountEl.GetProperty("value").GetString() ?? "0";
        if (!decimal.TryParse(valueStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            value = 0;

        var customId = cap.TryGetProperty("custom_id", out var c) ? c.GetString() : null;
        if (string.IsNullOrEmpty(customId) && unit0.TryGetProperty("custom_id", out var uc))
            customId = uc.GetString();

        return new PayPalCaptureResult(captureId, value, currency, customId ?? string.Empty);
    }
}

public sealed record PayPalCaptureResult(string CaptureId, decimal Amount, string CurrencyCode, string CustomId);