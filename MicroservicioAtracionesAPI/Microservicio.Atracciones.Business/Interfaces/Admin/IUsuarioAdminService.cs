using Microservicio.Atracciones.Business.DTOs.Admin.Usuarios;

namespace Microservicio.Atracciones.Business.Interfaces.Admin
{
    public interface IUsuarioAdminService
    {
        Task<UsuarioResponse> CrearAsync(CrearUsuarioRequest request, string usuarioAccion, string ip);
        Task<UsuarioResponse> ActualizarAsync(Guid usuGuid, ActualizarUsuarioRequest request, string usuarioAccion, string ip);
        Task EliminarAsync(Guid usuGuid, string usuarioAccion, string ip);
        Task<IReadOnlyList<UsuarioResponse>> ListarAsync();
    }
}
