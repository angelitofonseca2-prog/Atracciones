using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Atracciones.MsIdentidad.Api.Options;
using Atracciones.MsIdentidad.Api.Security;
using Atracciones.MsIdentidad.Business.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Atracciones.MsIdentidad.Api.Services;

public sealed class RsaJwtTokenIssuer : IJwtTokenIssuer
{
    private readonly JwtIssuerOptions _opts;
    private readonly IdentidadSigningKeys _keys;

    public RsaJwtTokenIssuer(IOptions<JwtIssuerOptions> opts, IdentidadSigningKeys keys)
    {
        _opts = opts.Value;
        _keys = keys;
    }

    public (string Token, DateTime ExpiraUtc) Emitir(UsuarioParaToken usuario)
    {
        var expira = DateTime.UtcNow.AddHours(_opts.ExpirationHours);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.UsuId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("D")),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new("usu_guid", usuario.UsuGuid.ToString("D")),
            new("login", usuario.Login),
        };

        if (usuario.CliId.HasValue)
            claims.Add(new Claim("cli_id", usuario.CliId.Value.ToString()));

        foreach (var rol in usuario.Roles)
            claims.Add(new Claim(ClaimTypes.Role, rol));

        var creds = new SigningCredentials(_keys.SigningKey, SecurityAlgorithms.RsaSha256);
        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            expires: expira,
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return (jwt, expira);
    }
}
