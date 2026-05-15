namespace Atracciones.MsClientes.Api.Options;

public sealed class JwtValidationOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string JwksUrl { get; set; } = string.Empty;
}
