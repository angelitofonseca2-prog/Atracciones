using Atracciones.MsIdentidad.Business.DTOs;

namespace Atracciones.MsIdentidad.Business.Interfaces;

public interface IAuthService
{
    Task<UsuarioAutenticadoDto> ValidarCredencialesAsync(LoginRequest request, CancellationToken ct = default);
}
