using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Atracciones.MsIdentidad.Api.Security;

public sealed class IdentidadSigningKeys : IDisposable
{
    private readonly RSA _rsa;

    public IdentidadSigningKeys(RSA rsa, string keyId)
    {
        _rsa = rsa;
        KeyId = keyId;
        SigningKey = new RsaSecurityKey(rsa) { KeyId = keyId };
    }

    public string KeyId { get; }
    public RsaSecurityKey SigningKey { get; }

    public string BuildJwksJson()
    {
        var p = _rsa.ExportParameters(false);
        var n = Base64UrlEncoder.Encode(p.Modulus ?? Array.Empty<byte>());
        var e = Base64UrlEncoder.Encode(p.Exponent ?? Array.Empty<byte>());
        return new StringBuilder()
            .Append("{\"keys\":[{\"kty\":\"RSA\",\"kid\":\"")
            .Append(KeyId.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal))
            .Append("\",\"use\":\"sig\",\"alg\":\"RS256\",\"n\":\"")
            .Append(n)
            .Append("\",\"e\":\"")
            .Append(e)
            .Append("\"}]}")
            .ToString();
    }

    public void Dispose() => _rsa.Dispose();
}

