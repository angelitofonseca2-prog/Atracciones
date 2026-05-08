using Microservicio.Atracciones.Business.Interfaces.Auth;

namespace Microservicio.Atracciones.Api.Services
{
    /// <summary>
    /// Implementación v1: comparación de texto plano (sin hash).
    /// Para habilitar BCrypt en producción, reemplazar el cuerpo de ambos
    /// métodos por BCrypt.Net.BCrypt.HashPassword / Verify.
    /// La interfaz IPasswordHasher (capa Business) no cambia.
    /// </summary>
    public class BcryptPasswordHasher : IPasswordHasher
    {
        public string Hashear(string password)
            => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        public bool Verificar(string password, string hash)
            => BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
