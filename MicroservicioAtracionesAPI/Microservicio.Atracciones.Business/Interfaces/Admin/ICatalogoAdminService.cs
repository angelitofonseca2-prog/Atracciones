using Microservicio.Atracciones.Business.DTOs.Admin.Catalogos;

namespace Microservicio.Atracciones.Business.Interfaces.Admin
{
    public interface ICatalogoAdminService
    {
        Task<IReadOnlyList<CategoriaResponse>> ListarCategoriasAsync();
        Task<CategoriaResponse> CrearCategoriaAsync(CrearCategoriaRequest request, string usuarioAccion, string ip);
        Task<CategoriaResponse> ActualizarCategoriaAsync(Guid catGuid, ActualizarCategoriaRequest request, string usuarioAccion, string ip);
        Task EliminarCategoriaAsync(Guid catGuid, string usuarioAccion, string ip);

        Task<IReadOnlyList<IdiomaResponse>> ListarIdiomasAsync();
        Task<IdiomaResponse> CrearIdiomaAsync(CrearIdiomaRequest request, string usuarioAccion, string ip);
        Task<IdiomaResponse> ActualizarIdiomaAsync(Guid idGuid, ActualizarIdiomaRequest request, string usuarioAccion, string ip);
        Task EliminarIdiomaAsync(Guid idGuid, string usuarioAccion, string ip);

        Task<IReadOnlyList<IncluyeResponse>> ListarIncluyeAsync();
        Task<IncluyeResponse> CrearIncluyeAsync(CrearIncluyeRequest request);
        Task<IncluyeResponse> ActualizarIncluyeAsync(Guid incGuid, ActualizarIncluyeRequest request);
        Task EliminarIncluyeAsync(Guid incGuid);
    }
}
