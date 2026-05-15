using Atracciones.Contracts.Catalogos.V1;
using Atracciones.MsAtracciones.DataManagement.Interfaces;
using Grpc.Core;

namespace Atracciones.MsAtracciones.Api.Grpc;

public sealed class CatalogGrpcService : CatalogoService.CatalogoServiceBase
{
    private readonly ICatalogosRepository _repo;

    public CatalogGrpcService(ICatalogosRepository repo) => _repo = repo;

    public override async Task<GetCatalogosPorGuidsResponse> GetCatalogosPorGuids(
        GetCatalogosPorGuidsRequest request,
        ServerCallContext context)
    {
        var ct = context.CancellationToken;
        var destinos = await _repo.GetDestinosByGuidsAsync(ParseGuids(request.DestinoGuids), ct);
        var categorias = await _repo.GetCategoriasByGuidsAsync(ParseGuids(request.CategoriaGuids), ct);
        var idiomas = await _repo.GetIdiomasByGuidsAsync(ParseGuids(request.IdiomaGuids), ct);
        var incluye = await _repo.GetIncluyeByGuidsAsync(ParseGuids(request.IncluyeGuids), ct);
        var imagenes = await _repo.GetImagenesByGuidsAsync(ParseGuids(request.ImagenGuids), ct);

        var resp = new GetCatalogosPorGuidsResponse();

        foreach (var d in destinos)
            resp.Destinos.Add(new DestinoGrpc
            {
                DesGuid = d.DesGuid.ToString(), Nombre = d.Nombre, Pais = d.Pais,
                ImagenUrl = d.ImagenUrl ?? string.Empty, Estado = d.Estado.ToString(),
            });

        foreach (var c in categorias)
            resp.Categorias.Add(new CategoriaGrpc
            {
                CatGuid = c.CatGuid.ToString(), Nombre = c.Nombre,
                ParentGuid = c.ParentGuid?.ToString() ?? string.Empty, Estado = c.Estado.ToString(),
            });

        foreach (var i in idiomas)
            resp.Idiomas.Add(new IdiomaGrpc
            {
                IdGuid = i.IdGuid.ToString(), Descripcion = i.Descripcion, Estado = i.Estado.ToString(),
            });

        foreach (var inc in incluye)
            resp.Incluye.Add(new IncluyeGrpc
            {
                IncGuid = inc.IncGuid.ToString(), Descripcion = inc.Descripcion, Estado = inc.Estado.ToString(),
            });

        foreach (var img in imagenes)
            resp.Imagenes.Add(new ImagenGrpc
            {
                ImgGuid = img.ImgGuid.ToString(), Url = img.Url,
                Descripcion = img.Descripcion ?? string.Empty, Estado = img.Estado.ToString(),
            });

        return resp;
    }

    private static List<Guid> ParseGuids(IEnumerable<string> raw)
        => raw.Select(s => Guid.TryParse(s, out var g) ? g : (Guid?)null)
              .Where(g => g.HasValue).Select(g => g!.Value).Distinct().ToList();
}
