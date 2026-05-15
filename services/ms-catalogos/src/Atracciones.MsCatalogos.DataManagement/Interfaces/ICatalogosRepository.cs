using Atracciones.MsCatalogos.DataManagement.Models;

namespace Atracciones.MsCatalogos.DataManagement.Interfaces;

public interface ICatalogosRepository
{
    Task<IReadOnlyList<DestinoDto>> ListDestinosActivosAsync(CancellationToken ct = default);
    Task<DestinoDto?> GetDestinoAsync(Guid guid, CancellationToken ct = default);
    Task UpsertDestinoAsync(Guid guid, string nombre, string pais, string? imagenUrl, char estado, string usuario, string ip, CancellationToken ct = default);

    Task<IReadOnlyList<CategoriaDto>> ListCategoriasActivasAsync(CancellationToken ct = default);
    Task<CategoriaDto?> GetCategoriaAsync(Guid guid, CancellationToken ct = default);
    Task UpsertCategoriaAsync(Guid guid, string nombre, Guid? parentGuid, char estado, string usuario, string ip, CancellationToken ct = default);

    Task<IReadOnlyList<IdiomaDto>> ListIdiomasActivosAsync(CancellationToken ct = default);
    Task<IdiomaDto?> GetIdiomaAsync(Guid guid, CancellationToken ct = default);
    Task<bool> IdiomaDescripcionExisteAsync(string descripcion, Guid? excluirGuid, CancellationToken ct = default);
    Task UpsertIdiomaAsync(Guid guid, string descripcion, char estado, string usuario, string ip, CancellationToken ct = default);

    Task<IReadOnlyList<IncluyeDto>> ListIncluyeActivosAsync(CancellationToken ct = default);
    Task<IncluyeDto?> GetIncluyeAsync(Guid guid, CancellationToken ct = default);
    Task UpsertIncluyeAsync(Guid guid, string descripcion, char estado, CancellationToken ct = default);

    Task<IReadOnlyList<ImagenDto>> ListImagenesActivasAsync(CancellationToken ct = default);
    Task<ImagenDto?> GetImagenAsync(Guid guid, CancellationToken ct = default);
    Task<bool> ImagenUrlExisteAsync(string url, Guid? excluirGuid, CancellationToken ct = default);
    Task UpsertImagenAsync(Guid guid, string url, string? descripcion, char estado, string usuario, string ip, CancellationToken ct = default);

    Task<IReadOnlyList<DestinoDto>> GetDestinosByGuidsAsync(IEnumerable<Guid> guids, CancellationToken ct = default);
    Task<IReadOnlyList<CategoriaDto>> GetCategoriasByGuidsAsync(IEnumerable<Guid> guids, CancellationToken ct = default);
    Task<IReadOnlyList<IdiomaDto>> GetIdiomasByGuidsAsync(IEnumerable<Guid> guids, CancellationToken ct = default);
    Task<IReadOnlyList<IncluyeDto>> GetIncluyeByGuidsAsync(IEnumerable<Guid> guids, CancellationToken ct = default);
    Task<IReadOnlyList<ImagenDto>> GetImagenesByGuidsAsync(IEnumerable<Guid> guids, CancellationToken ct = default);
}
