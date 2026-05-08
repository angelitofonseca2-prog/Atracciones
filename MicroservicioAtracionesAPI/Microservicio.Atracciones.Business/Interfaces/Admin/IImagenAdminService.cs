using Microservicio.Atracciones.Business.DTOs.Admin.Imagenes;

namespace Microservicio.Atracciones.Business.Interfaces.Admin
{
    public interface IImagenAdminService
    {
        Task<IReadOnlyList<ImagenResponse>> ListarAsync();
        Task<ImagenResponse> CrearAsync(CrearImagenRequest request, string usuarioAccion, string ip);
        Task<ImagenResponse> ActualizarAsync(Guid imgGuid, ActualizarImagenRequest request, string usuarioAccion, string ip);
        Task EliminarAsync(Guid imgGuid, string usuarioAccion, string ip);
    }
}
