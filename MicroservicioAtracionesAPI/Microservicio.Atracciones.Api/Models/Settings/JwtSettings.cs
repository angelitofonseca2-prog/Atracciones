namespace Microservicio.Atracciones.Api.Models.Settings;
public class JwtSettings
{
    /// <summary>Solo si <see cref="JwksUrl"/> está vacío (modo legado HS256).</summary>
    public string SecretKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    /// <summary>Si está definido, la validación JWT usa JWKS (RS256) de ms-identidad.</summary>
    public string? JwksUrl { get; set; }

    public int ExpirationHours { get; set; } = 8;
}

