using System.Security.Cryptography;
using Atracciones.MsIdentidad.Api.Options;
using Microsoft.Extensions.Options;

namespace Atracciones.MsIdentidad.Api.Security;

public static class IdentidadSigningKeysFactory
{
    public static IdentidadSigningKeys Create(
        IOptions<JwtIssuerOptions> jwtOptions,
        IHostEnvironment env,
        ILoggerFactory loggerFactory)
    {
        var opts = jwtOptions.Value;
        var logger = loggerFactory.CreateLogger("IdentidadSigningKeys");

        RSA rsa;
        if (!string.IsNullOrWhiteSpace(opts.RsaPrivateKeyPem))
        {
            rsa = RSA.Create();
            rsa.ImportFromPem(opts.RsaPrivateKeyPem.AsSpan());
        }
        else if (!string.IsNullOrWhiteSpace(opts.RsaPrivateKeyPath) && File.Exists(opts.RsaPrivateKeyPath))
        {
            rsa = RSA.Create();
            rsa.ImportFromPem(File.ReadAllText(opts.RsaPrivateKeyPath).AsSpan());
        }
        else if (env.IsDevelopment())
        {
            rsa = RSA.Create(2048);
            logger.LogWarning(
                "JWT RSA efímero (sin Jwt:RsaPrivateKeyPem ni RsaPrivateKeyPath). Los tokens se invalidan al reiniciar el servicio.");
        }
        else
            throw new InvalidOperationException(
                "Configure Jwt:RsaPrivateKeyPem o Jwt:RsaPrivateKeyPath para firmar JWT en producción.");

        return new IdentidadSigningKeys(rsa, opts.KeyId);
    }
}
