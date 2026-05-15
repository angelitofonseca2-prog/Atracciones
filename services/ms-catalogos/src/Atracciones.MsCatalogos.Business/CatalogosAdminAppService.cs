using Atracciones.MsCatalogos.Business.Dtos;
using Atracciones.MsCatalogos.Business.Exceptions;
using Atracciones.MsCatalogos.Business.Integration;
using Atracciones.MsCatalogos.DataManagement.Interfaces;
using Atracciones.MsCatalogos.DataManagement.Models;
using Microsoft.Extensions.Options;

namespace Atracciones.MsCatalogos.Business;

public interface ICatalogosAdminAppService
{
    Task<IReadOnlyList<DestinoResponseDto>> ListDestinosAsync(CancellationToken ct = default);
    Task<DestinoResponseDto> CrearDestinoAsync(CrearDestinoRequestDto req, string usuario, string ip, CancellationToken ct = default);
    Task<DestinoResponseDto> ActualizarDestinoAsync(Guid guid, ActualizarDestinoRequestDto req, string usuario, string ip, CancellationToken ct = default);
    Task EliminarDestinoAsync(Guid guid, string usuario, string ip, CancellationToken ct = default);

    Task<IReadOnlyList<CategoriaResponseDto>> ListCategoriasAsync(CancellationToken ct = default);
    Task<CategoriaResponseDto> CrearCategoriaAsync(CrearCategoriaRequestDto req, string usuario, string ip, CancellationToken ct = default);
    Task<CategoriaResponseDto> ActualizarCategoriaAsync(Guid guid, ActualizarCategoriaRequestDto req, string usuario, string ip, CancellationToken ct = default);
    Task EliminarCategoriaAsync(Guid guid, string usuario, string ip, CancellationToken ct = default);

    Task<IReadOnlyList<IdiomaResponseDto>> ListIdiomasAsync(CancellationToken ct = default);
    Task<IdiomaResponseDto> CrearIdiomaAsync(CrearIdiomaRequestDto req, string usuario, string ip, CancellationToken ct = default);
    Task<IdiomaResponseDto> ActualizarIdiomaAsync(Guid guid, ActualizarIdiomaRequestDto req, string usuario, string ip, CancellationToken ct = default);
    Task EliminarIdiomaAsync(Guid guid, string usuario, string ip, CancellationToken ct = default);

    Task<IReadOnlyList<IncluyeResponseDto>> ListIncluyeAsync(CancellationToken ct = default);
    Task<IncluyeResponseDto> CrearIncluyeAsync(CrearIncluyeRequestDto req, CancellationToken ct = default);
    Task<IncluyeResponseDto> ActualizarIncluyeAsync(Guid guid, ActualizarIncluyeRequestDto req, CancellationToken ct = default);
    Task EliminarIncluyeAsync(Guid guid, CancellationToken ct = default);

    Task<IReadOnlyList<ImagenResponseDto>> ListImagenesAsync(CancellationToken ct = default);
    Task<ImagenResponseDto> CrearImagenAsync(CrearImagenRequestDto req, string usuario, string ip, CancellationToken ct = default);
    Task<ImagenResponseDto> ActualizarImagenAsync(Guid guid, ActualizarImagenRequestDto req, string usuario, string ip, CancellationToken ct = default);
    Task EliminarImagenAsync(Guid guid, string usuario, string ip, CancellationToken ct = default);
}

public sealed class CatalogosAdminAppService : ICatalogosAdminAppService
{
    private readonly ICatalogosRepository _repo;
    private readonly IMonolithCatalogLegacyPublisher _publisher;
    private readonly MonolithCatalogLegacySyncOptions _syncOpts;

    public CatalogosAdminAppService(
        ICatalogosRepository repo,
        IMonolithCatalogLegacyPublisher publisher,
        IOptions<MonolithCatalogLegacySyncOptions> syncOpts)
    {
        _repo = repo;
        _publisher = publisher;
        _syncOpts = syncOpts.Value;
    }

    private Task MirrorAsync(CatalogMirrorBatch batch, CancellationToken ct)
        => _syncOpts.Enabled ? _publisher.PublishAsync(batch, ct) : Task.CompletedTask;

    private static DestinoResponseDto MapD(DestinoDto x) =>
        new(x.DesGuid.ToString(), x.Nombre, x.Pais, x.ImagenUrl, x.Estado);

    private static async Task<CategoriaResponseDto> MapC(ICatalogosRepository repo, CategoriaDto x, CancellationToken ct)
    {
        string? parentNombre = null;
        if (x.ParentGuid.HasValue)
        {
            var p = await repo.GetCategoriaAsync(x.ParentGuid.Value, ct);
            parentNombre = p?.Nombre;
        }

        return new CategoriaResponseDto(x.CatGuid.ToString(), x.Nombre, x.ParentGuid?.ToString(), parentNombre);
    }

    private static IdiomaResponseDto MapI(IdiomaDto x) => new(x.IdGuid.ToString(), x.Descripcion);
    private static IncluyeResponseDto MapInc(IncluyeDto x) => new(x.IncGuid.ToString(), x.Descripcion);
    private static ImagenResponseDto MapImg(ImagenDto x) =>
        new(x.ImgGuid.ToString(), x.Url, x.Descripcion, x.Estado, x.FechaIngreso);

    public async Task<IReadOnlyList<DestinoResponseDto>> ListDestinosAsync(CancellationToken ct = default)
        => (await _repo.ListDestinosActivosAsync(ct)).Select(MapD).ToList();

    public async Task<DestinoResponseDto> CrearDestinoAsync(CrearDestinoRequestDto req, string usuario, string ip, CancellationToken ct = default)
    {
        ValidarDestinoCrear(req);
        var g = Guid.NewGuid();
        await _repo.UpsertDestinoAsync(g, req.Nombre, req.Pais, req.ImagenUrl, 'A', usuario, ip, ct);
        var dto = await _repo.GetDestinoAsync(g, ct) ?? throw new InvalidOperationException("Destino no persistido.");
        await MirrorAsync(new CatalogMirrorBatch
        {
            Destinos = new List<DestinoMirrorRow> { new() { DesGuid = dto.DesGuid, Nombre = dto.Nombre, Pais = dto.Pais, ImagenUrl = dto.ImagenUrl, Estado = dto.Estado } },
        }, ct);
        return MapD(dto);
    }

    public async Task<DestinoResponseDto> ActualizarDestinoAsync(Guid guid, ActualizarDestinoRequestDto req, string usuario, string ip, CancellationToken ct = default)
    {
        var cur = await _repo.GetDestinoAsync(guid, ct) ?? throw new NotFoundException("Destino", guid);
        var nombre = req.Nombre ?? cur.Nombre;
        var pais = req.Pais ?? cur.Pais;
        var img = req.ImagenUrl ?? cur.ImagenUrl;
        if (!string.IsNullOrWhiteSpace(req.ImagenUrl) && !Uri.IsWellFormedUriString(req.ImagenUrl, UriKind.Absolute))
            throw new ValidationException(new[] { "La URL de imagen debe ser una URL absoluta válida." });
        await _repo.UpsertDestinoAsync(guid, nombre, pais, img, 'A', usuario, ip, ct);
        var dto = await _repo.GetDestinoAsync(guid, ct) ?? throw new NotFoundException("Destino", guid);
        await MirrorAsync(new CatalogMirrorBatch { Destinos = new List<DestinoMirrorRow> { new() { DesGuid = dto.DesGuid, Nombre = dto.Nombre, Pais = dto.Pais, ImagenUrl = dto.ImagenUrl, Estado = dto.Estado } } }, ct);
        return MapD(dto);
    }

    public async Task EliminarDestinoAsync(Guid guid, string usuario, string ip, CancellationToken ct = default)
    {
        var cur = await _repo.GetDestinoAsync(guid, ct) ?? throw new NotFoundException("Destino", guid);
        await _repo.UpsertDestinoAsync(guid, cur.Nombre, cur.Pais, cur.ImagenUrl, 'I', usuario, ip, ct);
        await MirrorAsync(new CatalogMirrorBatch { Destinos = new List<DestinoMirrorRow> { new() { DesGuid = guid, Nombre = cur.Nombre, Pais = cur.Pais, ImagenUrl = cur.ImagenUrl, Estado = 'I' } } }, ct);
    }

    public async Task<IReadOnlyList<CategoriaResponseDto>> ListCategoriasAsync(CancellationToken ct = default)
    {
        var list = await _repo.ListCategoriasActivasAsync(ct);
        var result = new List<CategoriaResponseDto>();
        foreach (var x in list)
            result.Add(await MapC(_repo, x, ct));
        return result;
    }

    public async Task<CategoriaResponseDto> CrearCategoriaAsync(CrearCategoriaRequestDto req, string usuario, string ip, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Nombre))
            throw new ValidationException(new[] { "El nombre de la categoría es obligatorio." });
        var g = Guid.NewGuid();
        await _repo.UpsertCategoriaAsync(g, req.Nombre, req.ParentGuid, 'A', usuario, ip, ct);
        var dto = await _repo.GetCategoriaAsync(g, ct) ?? throw new InvalidOperationException("Categoría no persistida.");
        await MirrorAsync(new CatalogMirrorBatch
        {
            Categorias = new List<CategoriaMirrorRow> { new() { CatGuid = dto.CatGuid, Nombre = dto.Nombre, ParentGuid = dto.ParentGuid, Estado = dto.Estado } },
        }, ct);
        return await MapC(_repo, dto, ct);
    }

    public async Task<CategoriaResponseDto> ActualizarCategoriaAsync(Guid guid, ActualizarCategoriaRequestDto req, string usuario, string ip, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Nombre))
            throw new ValidationException(new[] { "El nombre de la categoría es obligatorio." });
        var cur = await _repo.GetCategoriaAsync(guid, ct) ?? throw new NotFoundException("Categoría", guid);
        await _repo.UpsertCategoriaAsync(guid, req.Nombre, req.ParentGuid, 'A', usuario, ip, ct);
        var dto = await _repo.GetCategoriaAsync(guid, ct) ?? throw new NotFoundException("Categoría", guid);
        await MirrorAsync(new CatalogMirrorBatch { Categorias = new List<CategoriaMirrorRow> { new() { CatGuid = dto.CatGuid, Nombre = dto.Nombre, ParentGuid = dto.ParentGuid, Estado = dto.Estado } } }, ct);
        return await MapC(_repo, dto, ct);
    }

    public async Task EliminarCategoriaAsync(Guid guid, string usuario, string ip, CancellationToken ct = default)
    {
        var cur = await _repo.GetCategoriaAsync(guid, ct) ?? throw new NotFoundException("Categoría", guid);
        await _repo.UpsertCategoriaAsync(guid, cur.Nombre, cur.ParentGuid, 'I', usuario, ip, ct);
        await MirrorAsync(new CatalogMirrorBatch { Categorias = new List<CategoriaMirrorRow> { new() { CatGuid = guid, Nombre = cur.Nombre, ParentGuid = cur.ParentGuid, Estado = 'I' } } }, ct);
    }

    public async Task<IReadOnlyList<IdiomaResponseDto>> ListIdiomasAsync(CancellationToken ct = default)
        => (await _repo.ListIdiomasActivosAsync(ct)).Select(MapI).ToList();

    public async Task<IdiomaResponseDto> CrearIdiomaAsync(CrearIdiomaRequestDto req, string usuario, string ip, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Descripcion))
            throw new ValidationException(new[] { "La descripción del idioma es obligatoria." });
        var desc = req.Descripcion.Trim();
        if (await _repo.IdiomaDescripcionExisteAsync(desc, null, ct))
            throw new ConflictException($"Ya existe un idioma con la descripción '{desc}'.");
        var g = Guid.NewGuid();
        await _repo.UpsertIdiomaAsync(g, desc, 'A', usuario, ip, ct);
        var dto = await _repo.GetIdiomaAsync(g, ct) ?? throw new InvalidOperationException("Idioma no persistido.");
        await MirrorAsync(new CatalogMirrorBatch { Idiomas = new List<IdiomaMirrorRow> { new() { IdGuid = dto.IdGuid, Descripcion = dto.Descripcion, Estado = dto.Estado } } }, ct);
        return MapI(dto);
    }

    public async Task<IdiomaResponseDto> ActualizarIdiomaAsync(Guid guid, ActualizarIdiomaRequestDto req, string usuario, string ip, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Descripcion))
            throw new ValidationException(new[] { "La descripción del idioma es obligatoria." });
        var desc = req.Descripcion.Trim();
        var cur = await _repo.GetIdiomaAsync(guid, ct) ?? throw new NotFoundException("Idioma", guid);
        if (await _repo.IdiomaDescripcionExisteAsync(desc, guid, ct))
            throw new ConflictException($"Ya existe un idioma con la descripción '{desc}'.");
        await _repo.UpsertIdiomaAsync(guid, desc, 'A', usuario, ip, ct);
        var dto = await _repo.GetIdiomaAsync(guid, ct) ?? throw new NotFoundException("Idioma", guid);
        await MirrorAsync(new CatalogMirrorBatch { Idiomas = new List<IdiomaMirrorRow> { new() { IdGuid = dto.IdGuid, Descripcion = dto.Descripcion, Estado = dto.Estado } } }, ct);
        return MapI(dto);
    }

    public async Task EliminarIdiomaAsync(Guid guid, string usuario, string ip, CancellationToken ct = default)
    {
        var cur = await _repo.GetIdiomaAsync(guid, ct) ?? throw new NotFoundException("Idioma", guid);
        await _repo.UpsertIdiomaAsync(guid, cur.Descripcion, 'I', usuario, ip, ct);
        await MirrorAsync(new CatalogMirrorBatch { Idiomas = new List<IdiomaMirrorRow> { new() { IdGuid = guid, Descripcion = cur.Descripcion, Estado = 'I' } } }, ct);
    }

    public async Task<IReadOnlyList<IncluyeResponseDto>> ListIncluyeAsync(CancellationToken ct = default)
        => (await _repo.ListIncluyeActivosAsync(ct)).Select(MapInc).ToList();

    public async Task<IncluyeResponseDto> CrearIncluyeAsync(CrearIncluyeRequestDto req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Descripcion))
            throw new ValidationException(new[] { "La descripción del elemento incluido es obligatoria." });
        var g = Guid.NewGuid();
        await _repo.UpsertIncluyeAsync(g, req.Descripcion, 'A', ct);
        var dto = await _repo.GetIncluyeAsync(g, ct) ?? throw new InvalidOperationException("Incluye no persistido.");
        await MirrorAsync(new CatalogMirrorBatch { Incluye = new List<IncluyeMirrorRow> { new() { IncGuid = dto.IncGuid, Descripcion = dto.Descripcion, Estado = dto.Estado } } }, ct);
        return MapInc(dto);
    }

    public async Task<IncluyeResponseDto> ActualizarIncluyeAsync(Guid guid, ActualizarIncluyeRequestDto req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Descripcion))
            throw new ValidationException(new[] { "La descripción del elemento incluido es obligatoria." });
        _ = await _repo.GetIncluyeAsync(guid, ct) ?? throw new NotFoundException("Incluye", guid);
        await _repo.UpsertIncluyeAsync(guid, req.Descripcion, 'A', ct);
        var dto = await _repo.GetIncluyeAsync(guid, ct) ?? throw new NotFoundException("Incluye", guid);
        await MirrorAsync(new CatalogMirrorBatch { Incluye = new List<IncluyeMirrorRow> { new() { IncGuid = dto.IncGuid, Descripcion = dto.Descripcion, Estado = dto.Estado } } }, ct);
        return MapInc(dto);
    }

    public async Task EliminarIncluyeAsync(Guid guid, CancellationToken ct = default)
    {
        var cur = await _repo.GetIncluyeAsync(guid, ct) ?? throw new NotFoundException("Incluye", guid);
        await _repo.UpsertIncluyeAsync(guid, cur.Descripcion, 'I', ct);
        await MirrorAsync(new CatalogMirrorBatch { Incluye = new List<IncluyeMirrorRow> { new() { IncGuid = guid, Descripcion = cur.Descripcion, Estado = 'I' } } }, ct);
    }

    public async Task<IReadOnlyList<ImagenResponseDto>> ListImagenesAsync(CancellationToken ct = default)
        => (await _repo.ListImagenesActivasAsync(ct)).Select(MapImg).ToList();

    public async Task<ImagenResponseDto> CrearImagenAsync(CrearImagenRequestDto req, string usuario, string ip, CancellationToken ct = default)
    {
        ValidarImagenCrear(req);
        if (await _repo.ImagenUrlExisteAsync(req.Url, null, ct))
            throw new ConflictException("Ya existe una imagen activa con la misma URL.");
        var g = Guid.NewGuid();
        await _repo.UpsertImagenAsync(g, req.Url, req.Descripcion, 'A', usuario, ip, ct);
        var dto = await _repo.GetImagenAsync(g, ct) ?? throw new InvalidOperationException("Imagen no persistida.");
        await MirrorAsync(new CatalogMirrorBatch { Imagenes = new List<ImagenMirrorRow> { new() { ImgGuid = dto.ImgGuid, Url = dto.Url, Descripcion = dto.Descripcion, Estado = dto.Estado, FechaIngreso = dto.FechaIngreso } } }, ct);
        return MapImg(dto);
    }

    public async Task<ImagenResponseDto> ActualizarImagenAsync(Guid guid, ActualizarImagenRequestDto req, string usuario, string ip, CancellationToken ct = default)
    {
        var cur = await _repo.GetImagenAsync(guid, ct) ?? throw new NotFoundException("Imagen", guid);
        var url = req.Url ?? cur.Url;
        var desc = req.Descripcion ?? cur.Descripcion;
        if (req.Url is not null && !Uri.IsWellFormedUriString(req.Url, UriKind.Absolute))
            throw new ValidationException(new[] { "La URL debe ser absoluta válida." });
        if (await _repo.ImagenUrlExisteAsync(url, guid, ct))
            throw new ConflictException("Ya existe una imagen activa con la misma URL.");
        await _repo.UpsertImagenAsync(guid, url, desc, 'A', usuario, ip, ct);
        var dto = await _repo.GetImagenAsync(guid, ct) ?? throw new NotFoundException("Imagen", guid);
        await MirrorAsync(new CatalogMirrorBatch { Imagenes = new List<ImagenMirrorRow> { new() { ImgGuid = dto.ImgGuid, Url = dto.Url, Descripcion = dto.Descripcion, Estado = dto.Estado, FechaIngreso = dto.FechaIngreso } } }, ct);
        return MapImg(dto);
    }

    public async Task EliminarImagenAsync(Guid guid, string usuario, string ip, CancellationToken ct = default)
    {
        var cur = await _repo.GetImagenAsync(guid, ct) ?? throw new NotFoundException("Imagen", guid);
        await _repo.UpsertImagenAsync(guid, cur.Url, cur.Descripcion, 'I', usuario, ip, ct);
        await MirrorAsync(new CatalogMirrorBatch { Imagenes = new List<ImagenMirrorRow> { new() { ImgGuid = guid, Url = cur.Url, Descripcion = cur.Descripcion, Estado = 'I', FechaIngreso = cur.FechaIngreso } } }, ct);
    }

    private static void ValidarDestinoCrear(CrearDestinoRequestDto req)
    {
        var e = new List<string>();
        if (string.IsNullOrWhiteSpace(req.Nombre)) e.Add("El nombre del destino es obligatorio.");
        if (string.IsNullOrWhiteSpace(req.Pais)) e.Add("El país del destino es obligatorio.");
        if (!string.IsNullOrWhiteSpace(req.ImagenUrl) && !Uri.IsWellFormedUriString(req.ImagenUrl, UriKind.Absolute))
            e.Add("La URL de imagen debe ser una URL absoluta válida.");
        if (e.Count > 0) throw new ValidationException(e);
    }

    private static void ValidarImagenCrear(CrearImagenRequestDto req)
    {
        var e = new List<string>();
        if (string.IsNullOrWhiteSpace(req.Url)) e.Add("La URL es obligatoria.");
        else if (!Uri.IsWellFormedUriString(req.Url.Trim(), UriKind.Absolute))
            e.Add("La URL debe ser absoluta válida.");
        if (e.Count > 0) throw new ValidationException(e);
    }
}
