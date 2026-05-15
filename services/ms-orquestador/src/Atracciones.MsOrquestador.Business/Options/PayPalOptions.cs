namespace Atracciones.MsOrquestador.Business.Options;

public sealed class PayPalOptions
{
    public const string SectionName = "PayPal";

    /// <summary>OAuth y Orders API (p. ej. https://api-m.sandbox.paypal.com o https://api-m.paypal.com).</summary>
    public string BaseUrl { get; set; } = "https://api-m.sandbox.paypal.com";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>ID del webhook configurado en el panel de PayPal (verificación de firma).</summary>
    public string WebhookId { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId)
        && !string.IsNullOrWhiteSpace(ClientSecret);
}

