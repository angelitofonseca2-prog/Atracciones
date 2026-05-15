using Atracciones.Contracts.Catalogos.V1;
using Atracciones.MsAtracciones.DataManagement.Interfaces;

namespace Atracciones.MsAtracciones.Business.Integration;

public interface ICatalogoGrpcClient
{
    Task<GetCatalogosPorGuidsResponse> GetCatalogosPorGuidsAsync(GetCatalogosPorGuidsRequest request, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, CategoriaGrpc>> ObtenerCategoriasConAncestrosAsync(
        IEnumerable<Guid> categoriasIniciales,
        CancellationToken ct = default);
}

/// <summary>
/// Implementación local de ICatalogoGrpcClient que consulta directamente
/// el CatalogosDbContext (mismo proceso) en lugar de hacer una llamada gRPC de red.
/// </summary>
public sealed class CatalogoLocalClient : ICatalogoGrpcClient
{
    private readonly ICatalogosRepository _repo;

    public CatalogoLocalClient(ICatalogosRepository repo) => _repo = repo;

    public async Task<GetCatalogosPorGuidsResponse> GetCatalogosPorGuidsAsync(
        GetCatalogosPorGuidsRequest request,
        CancellationToken ct = default)
    {
        var destinoGuids = ParseGuids(request.DestinoGuids);
        var catGuids = ParseGuids(request.CategoriaGuids);
        var idiomaGuids = ParseGuids(request.IdiomaGuids);
        var incluyeGuids = ParseGuids(request.IncluyeGuids);
        var imagenGuids = ParseGuids(request.ImagenGuids);

        var destinos = await _repo.GetDestinosByGuidsAsync(destinoGuids, ct);
        var categorias = await _repo.GetCategoriasByGuidsAsync(catGuids, ct);
        var idiomas = await _repo.GetIdiomasByGuidsAsync(idiomaGuids, ct);
        var incluye = await _repo.GetIncluyeByGuidsAsync(incluyeGuids, ct);
        var imagenes = await _repo.GetImagenesByGuidsAsync(imagenGuids, ct);

        var resp = new GetCatalogosPorGuidsResponse();

        foreach (var d in destinos)
            resp.Destinos.Add(new DestinoGrpc
            {
                DesGuid = d.DesGuid.ToString(),
                Nombre = d.Nombre,
                Pais = d.Pais,
                ImagenUrl = d.ImagenUrl ?? string.Empty,
                Estado = d.Estado.ToString(),
            });

        foreach (var c in categorias)
            resp.Categorias.Add(new CategoriaGrpc
            {
                CatGuid = c.CatGuid.ToString(),
                Nombre = c.Nombre,
                ParentGuid = c.ParentGuid?.ToString() ?? string.Empty,
                Estado = c.Estado.ToString(),
            });

        foreach (var i in idiomas)
            resp.Idiomas.Add(new IdiomaGrpc
            {
                IdGuid = i.IdGuid.ToString(),
                Descripcion = i.Descripcion,
                Estado = i.Estado.ToString(),
            });

        foreach (var inc in incluye)
            resp.Incluye.Add(new IncluyeGrpc
            {
                IncGuid = inc.IncGuid.ToString(),
                Descripcion = inc.Descripcion,
                Estado = inc.Estado.ToString(),
            });

        foreach (var img in imagenes)
            resp.Imagenes.Add(new ImagenGrpc
            {
                ImgGuid = img.ImgGuid.ToString(),
                Url = img.Url,
                Descripcion = img.Descripcion ?? string.Empty,
                Estado = img.Estado.ToString(),
            });

        return resp;
    }

    public async Task<IReadOnlyDictionary<Guid, CategoriaGrpc>> ObtenerCategoriasConAncestrosAsync(
        IEnumerable<Guid> categoriasIniciales,
        CancellationToken ct = default)
    {
        var pending = new HashSet<Guid>(categoriasIniciales);
        var merged = new Dictionary<Guid, CategoriaGrpc>();

        for (var depth = 0; depth < 12 && pending.Count > 0; depth++)
        {
            var cats = await _repo.GetCategoriasByGuidsAsync(pending, ct);
            pending.Clear();

            foreach (var c in cats)
            {
                if (merged.ContainsKey(c.CatGuid)) continue;
                merged[c.CatGuid] = new CategoriaGrpc
                {
                    CatGuid = c.CatGuid.ToString(),
                    Nombre = c.Nombre,
                    ParentGuid = c.ParentGuid?.ToString() ?? string.Empty,
                    Estado = c.Estado.ToString(),
                };
                if (c.ParentGuid.HasValue && !merged.ContainsKey(c.ParentGuid.Value))
                    pending.Add(c.ParentGuid.Value);
            }
        }

        return merged;
    }

    private static List<Guid> ParseGuids(IEnumerable<string> raw)
        => raw.Select(s => Guid.TryParse(s, out var g) ? g : (Guid?)null)
              .Where(g => g.HasValue)
              .Select(g => g!.Value)
              .Distinct()
              .ToList();
}
