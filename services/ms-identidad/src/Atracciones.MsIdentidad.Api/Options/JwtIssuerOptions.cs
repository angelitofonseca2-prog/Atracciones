namespace Atracciones.MsIdentidad.Api.Options;

public sealed class JwtIssuerOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationHours { get; set; } = 8;
    public string KeyId { get; set; } = "atracciones-identidad-key-1";
    public string? RsaPrivateKeyPem { get; set; }
    public string? RsaPrivateKeyPath { get; set; }
}
