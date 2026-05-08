using Microservicio.Atracciones.Api.Models.Settings;
using Microservicio.Atracciones.DataManagement.Models.Seguridad;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microservicio.Atracciones.Api.Services;

namespace Microservicio.Atracciones.Api.Services
{
    public class JwtTokenService : TokenService
    {
        private readonly JwtSettings _settings;

        public JwtTokenService(IOptions<JwtSettings> settings)
            => _settings = settings.Value;

        public (string Token, DateTime Expiracion) GenerarToken(LoginDataModel model)
        {
            var expiracion = DateTime.UtcNow.AddHours(_settings.ExpirationHours);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,  model.UsuId.ToString()),
            new(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,  DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new("usu_guid", model.UsuGuid.ToString()),
            new("login",    model.UsuLogin)
        };

            // E-05: Inyectar ID de cliente para que las reservas/reseñas lo reconozcan
            if (model.CliId.HasValue)
                claims.Add(new Claim("cli_id", model.CliId.Value.ToString()));

            // Agrega un claim por cada rol
            foreach (var rol in model.Roles)
                claims.Add(new Claim(ClaimTypes.Role, rol));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expiracion,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiracion);
        }
    }
}
