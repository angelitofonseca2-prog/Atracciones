using System.ComponentModel.DataAnnotations;
using Atracciones.Contracts.Catalogos.V1;
using Atracciones.MsAtracciones.Business.Common;
using Atracciones.MsAtracciones.Business.Dtos.Admin.Atracciones;
using Atracciones.MsAtracciones.Business.Exceptions;
using DomainValidationException = Atracciones.MsAtracciones.Business.Exceptions.ValidationException;
using Atracciones.MsAtracciones.Business.Integration;
using Atracciones.MsAtracciones.DataManagement.Interfaces;
using Atracciones.MsAtracciones.DataManagement.Models;

namespace Atracciones.MsAtracciones.Business.Services;

public sealed class InventarioAdminAppService : IInventarioAdminAppService
{
    private readonly IInventarioRepository _repo;
    private readonly ICatalogoGrpcClient _catalog;

    public InventarioAdminAppService(IInventarioRepository repo, ICatalogoGrpcClient catalog)
    {
        _repo = repo;
        _catalog = catalog;
    }

    public async Task<DataPagedResult<AtraccionAdminResponse>> ListarAsync(AtraccionAdminFiltroRequest filtro, CancellationToken ct = default)
    {
        Validar(filtro);
        var q = new AtraccionAdminFiltroQuery(filtro.Page, filtro.Limit, filtro.Busqueda);
        var paged = await _repo.ListarAtraccionesAdminAsync(q, ct);
        var items = new List<AtraccionAdminResponse>();
        foreach (var row in paged.Items)
        {
            var full = await _repo.ObtenerAtraccionAdminCompletaAsync(row.AtGuid, ct);
            if (full is not null)
                items.Add(await ToResponseAsync(full, ct));
        }

        return new DataPagedResult<AtraccionAdminResponse>(items, paged.TotalFiltrado, paged.TotalSinFiltros, filtro.Page, filtro.Limit);
    }

    public async Task<AtraccionAdminResponse> ObtenerPorGuidAsync(Guid atGuid, CancellationToken ct = default)
    {
        var full = await _repo.ObtenerAtraccionAdminCompletaAsync(atGuid, ct)
            ?? throw new NotFoundException("Atracción", atGuid);
        return await ToResponseAsync(full, ct);
    }

    public async Task<AtraccionAdminResponse> CrearAsync(CrearAtraccionRequest request, string usuario, string ip, CancellationToken ct = default)
    {
        Validar(request);
        var snap = await CargarSnapshotsCatalogoAsync(
            request.DestinoGuid,
            request.CategoriaGuids,
            request.IdiomaGuids,
            request.ImagenGuids,
            request.IncluyeGuids,
            ct);

        var model = new AtraccionPersistModel
        {
            DesGuid = request.DestinoGuid,
            DesNombreSnap = snap.Destino.Nombre,
            DesPaisSnap = snap.Destino.Pais,
            NumEstablecimiento = request.NumEstablecimiento,
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Direccion = request.Direccion,
            DuracionMinutos = request.DuracionMinutos,
            PuntoEncuentro = request.PuntoEncuentro,
            PrecioReferencia = request.PrecioReferencia,
            Disponible = true,
            Usuario = usuario,
            Ip = ip,
            CategoriaGuids = request.CategoriaGuids.Distinct().ToList(),
            IdiomaGuids = request.IdiomaGuids.Distinct().ToList(),
            ImagenGuids = request.ImagenGuids.Distinct().ToList(),
            IncluyeGuids = request.IncluyeGuids.Distinct().ToList(),
            IdiomaDescripciones = snap.IdiomaDesc,
            ImagenUrls = snap.ImagenUrl,
            IncluyeDescripciones = snap.IncluyeDesc,
        };

        var nuevoGuid = await _repo.CrearAtraccionConRelacionesAsync(model, ct);

        var created = await _repo.ObtenerAtraccionAdminCompletaAsync(nuevoGuid, ct)
            ?? throw new InvalidOperationException("No se pudo leer la atracción creada.");

        return await ToResponseAsync(created, ct);
    }

    public async Task<AtraccionAdminResponse> ActualizarAsync(Guid atGuid, ActualizarAtraccionRequest request, string usuario, string ip, CancellationToken ct = default)
    {
        Validar(request);
        var full = await _repo.ObtenerAtraccionAdminCompletaAsync(atGuid, ct)
            ?? throw new NotFoundException("Atracción", atGuid);

        var desGuid = request.DestinoGuid ?? full.Base.DesGuid;
        var cats = request.CategoriaGuids?.Distinct().ToList() ?? full.CategoriaGuids.ToList();
        var idiomas = request.IdiomaGuids?.Distinct().ToList() ?? full.IdiomaGuids.ToList();
        var imgs = request.ImagenGuids?.Distinct().ToList() ?? full.ImagenGuids.ToList();
        var incs = request.IncluyeGuids?.Distinct().ToList() ?? full.IncluyeGuids.ToList();

        var snap = await CargarSnapshotsCatalogoAsync(desGuid, cats, idiomas, imgs, incs, ct);

        var model = new AtraccionPersistModel
        {
            AtGuid = atGuid,
            DesGuid = desGuid,
            DesNombreSnap = snap.Destino.Nombre,
            DesPaisSnap = snap.Destino.Pais,
            NumEstablecimiento = request.NumEstablecimiento ?? full.Base.AtNumEstablecimiento,
            Nombre = request.Nombre ?? full.Base.AtNombre,
            Descripcion = request.Descripcion ?? full.Base.AtDescripcion,
            Direccion = request.Direccion ?? full.Base.AtDireccion,
            DuracionMinutos = request.DuracionMinutos ?? full.Base.AtDuracionMinutos,
            PuntoEncuentro = request.PuntoEncuentro ?? full.Base.AtPuntoEncuentro,
            PrecioReferencia = request.PrecioReferencia ?? full.Base.AtPrecioReferencia,
            Disponible = request.Disponible ?? full.Base.AtDisponible,
            Usuario = usuario,
            Ip = ip,
            CategoriaGuids = cats,
            IdiomaGuids = idiomas,
            ImagenGuids = imgs,
            IncluyeGuids = incs,
            IdiomaDescripciones = snap.IdiomaDesc,
            ImagenUrls = snap.ImagenUrl,
            IncluyeDescripciones = snap.IncluyeDesc,
        };

        await _repo.ActualizarAtraccionConRelacionesAsync(model, ct);

        var updated = await _repo.ObtenerAtraccionAdminCompletaAsync(atGuid, ct)
            ?? throw new NotFoundException("Atracción", atGuid);
        return await ToResponseAsync(updated, ct);
    }

    public async Task EliminarAsync(Guid atGuid, string usuario, string ip, CancellationToken ct = default)
    {
        _ = await _repo.ObtenerAtraccionAdminCompletaAsync(atGuid, ct)
            ?? throw new NotFoundException("Atracción", atGuid);
        await _repo.EliminarAtraccionLogicoAsync(atGuid, usuario, ip, ct);
    }

    private async Task<AtraccionAdminResponse> ToResponseAsync(AtraccionAdminCompletaRow row, CancellationToken ct)
    {
        var req = new GetCatalogosPorGuidsRequest();
        foreach (var g in row.CategoriaGuids.Distinct())
            req.CategoriaGuids.Add(g.ToString());

        var resp = await _catalog.GetCatalogosPorGuidsAsync(req, ct);
        var catNomByGuid = resp.Categorias
            .Where(c => string.Equals(c.Estado, "A", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(c => Guid.Parse(c.CatGuid), c => c.Nombre);

        return new AtraccionAdminResponse
        {
            AtGuid = row.Base.AtGuid.ToString(),
            DestinoGuid = row.Base.DesGuid.ToString(),
            NumEstablecimiento = row.Base.AtNumEstablecimiento,
            Nombre = row.Base.AtNombre,
            Ciudad = row.Base.DesNombreSnap,
            Pais = row.Base.DesPaisSnap,
            Descripcion = row.Base.AtDescripcion,
            Direccion = row.Base.AtDireccion,
            DuracionMinutos = row.Base.AtDuracionMinutos,
            PuntoEncuentro = row.Base.AtPuntoEncuentro,
            PrecioReferencia = row.Base.AtPrecioReferencia,
            Disponible = row.Base.AtDisponible,
            Estado = row.Base.AtEstado,
            TotalResenias = row.Base.AtTotalResenias,
            FechaIngreso = row.Base.AtFechaIngreso,
            ImagenPrincipal = row.ImagenUrls.FirstOrDefault(),
            CategoriaGuids = row.CategoriaGuids.Select(g => g.ToString()).ToList(),
            IdiomaGuids = row.IdiomaGuids.Select(g => g.ToString()).ToList(),
            ImagenGuids = row.ImagenGuids.Select(g => g.ToString()).ToList(),
            IncluyeGuids = row.IncluyeGuids.Select(g => g.ToString()).ToList(),
            Idiomas = row.IdiomaDescripciones.ToList(),
            Categorias = row.CategoriaGuids.Select(g => catNomByGuid.GetValueOrDefault(g, g.ToString())).ToList(),
            Imagenes = row.ImagenUrls.ToList(),
            Incluyes = row.IncluyeDescripciones.ToList(),
        };
    }

    private async Task<(DestinoGrpc Destino, Dictionary<Guid, string> IdiomaDesc, Dictionary<Guid, string> ImagenUrl, Dictionary<Guid, string> IncluyeDesc)>
        CargarSnapshotsCatalogoAsync(
            Guid desGuid,
            IList<Guid> categorias,
            IList<Guid> idiomas,
            IList<Guid> imagenes,
            IList<Guid> incluye,
            CancellationToken ct)
    {
        var req = new GetCatalogosPorGuidsRequest();
        req.DestinoGuids.Add(desGuid.ToString());
        foreach (var c in categorias.Distinct()) req.CategoriaGuids.Add(c.ToString());
        foreach (var i in idiomas.Distinct()) req.IdiomaGuids.Add(i.ToString());
        foreach (var i in imagenes.Distinct()) req.ImagenGuids.Add(i.ToString());
        foreach (var i in incluye.Distinct()) req.IncluyeGuids.Add(i.ToString());

        var resp = await _catalog.GetCatalogosPorGuidsAsync(req, ct);

        var dest = resp.Destinos.FirstOrDefault(d => d.DesGuid == desGuid.ToString());
        if (dest is null || !string.Equals(dest.Estado, "A", StringComparison.OrdinalIgnoreCase))
            throw new NotFoundException($"Destino '{desGuid}' no encontrado o inactivo.");

        var idiomaDesc = new Dictionary<Guid, string>();
        foreach (var g in idiomas.Distinct())
        {
            var e = resp.Idiomas.FirstOrDefault(i => i.IdGuid == g.ToString());
            if (e is null || !string.Equals(e.Estado, "A", StringComparison.OrdinalIgnoreCase))
                throw new NotFoundException($"Idioma '{g}' no encontrado o inactivo.");
            idiomaDesc[g] = e.Descripcion;
        }

        var imgUrl = new Dictionary<Guid, string>();
        foreach (var g in imagenes.Distinct())
        {
            var e = resp.Imagenes.FirstOrDefault(i => i.ImgGuid == g.ToString());
            if (e is null || !string.Equals(e.Estado, "A", StringComparison.OrdinalIgnoreCase))
                throw new NotFoundException($"Imagen '{g}' no encontrada o inactiva.");
            imgUrl[g] = e.Url;
        }

        var incDesc = new Dictionary<Guid, string>();
        foreach (var g in incluye.Distinct())
        {
            var e = resp.Incluye.FirstOrDefault(i => i.IncGuid == g.ToString());
            if (e is null || !string.Equals(e.Estado, "A", StringComparison.OrdinalIgnoreCase))
                throw new NotFoundException($"Ítem incluye '{g}' no encontrado o inactivo.");
            incDesc[g] = e.Descripcion;
        }

        foreach (var g in categorias.Distinct())
        {
            var e = resp.Categorias.FirstOrDefault(i => i.CatGuid == g.ToString());
            if (e is null || !string.Equals(e.Estado, "A", StringComparison.OrdinalIgnoreCase))
                throw new NotFoundException($"Categoría '{g}' no encontrada o inactiva.");
        }

        return (dest, idiomaDesc, imgUrl, incDesc);
    }

    private static void Validar(object o)
    {
        var ctx = new ValidationContext(o);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(o, ctx, results, true))
            throw new DomainValidationException(results.Select(r => r.ErrorMessage ?? "inválido").ToList());
    }
}
