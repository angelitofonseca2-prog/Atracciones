using Microsoft.IdentityModel.Tokens;

namespace Atracciones.MsAtracciones.Api.Security;

public sealed class JwksKeyStore
{
    private JsonWebKeySet? _jwks;

    public async Task LoadAsync(string url, CancellationToken ct = default)
    {
        using var hc = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        var json = await hc.GetStringAsync(new Uri(url), ct);
        _jwks = new JsonWebKeySet(json);
    }

    public IEnumerable<SecurityKey> ResolveSigningKeys(string? kid)
    {
        if (_jwks is null)
            return [];

        if (string.IsNullOrEmpty(kid))
            return _jwks.GetSigningKeys();

        return _jwks.GetSigningKeys().Where(k => k.KeyId == kid);
    }
}
