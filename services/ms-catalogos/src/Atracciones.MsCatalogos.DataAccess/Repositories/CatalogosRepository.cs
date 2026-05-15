using Atracciones.MsCatalogos.DataAccess.Context;
using Atracciones.MsCatalogos.DataAccess.Entities;
using Atracciones.MsCatalogos.DataManagement.Interfaces;
using Atracciones.MsCatalogos.DataManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsCatalogos.DataAccess.Repositories;

public sealed class CatalogosRepository : ICatalogosRepository
{
    private readonly CatalogosDbContext _db;

    public CatalogosRepository(CatalogosDbContext db) => _db = db;

    private static DestinoDto Map(DestinoEntity x) => new(x.DesGuid, x.DesNombre, x.DesPais, x.DesImagenUrl, x.DesEstado);

    private static CategoriaDto Map(CategoriaEntity x) => new(x.CatGuid, x.CatNombre, x.CatParentGuid, x.CatEstado);

    private static IdiomaDto Map(IdiomaEntity x) => new(x.IdGuid, x.IdDescripcion, x.IdEstado);

    private static IncluyeDto Map(IncluyeEntity x) => new(x.IncGuid, x.IncDescripcion, x.IncEstado);

    private static ImagenDto Map(ImagenEntity x) =>
        new(x.ImgGuid, x.ImgUrl, x.ImgDescripcion, x.ImgEstado, x.ImgFechaIngreso);

    public async Task<IReadOnlyList<DestinoDto>> ListDestinosActivosAsync(CancellationToken ct = default)
    {
        var rows = await _db.Destinos.AsNoTracking().Where(x => x.DesEstado == 'A').OrderBy(x => x.DesNombre).ToListAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<DestinoDto?> GetDestinoAsync(Guid guid, CancellationToken ct = default)
    {
        var x = await _db.Destinos.AsNoTracking().FirstOrDefaultAsync(e => e.DesGuid == guid && e.DesEstado == 'A', ct);
        return x is null ? null : Map(x);
    }

    public async Task UpsertDestinoAsync(Guid guid, string nombre, string pais, string? imagenUrl, char estado, string usuario, string ip, CancellationToken ct = default)
    {
        var entity = await _db.Destinos.FirstOrDefaultAsync(x => x.DesGuid == guid, ct);
        if (entity is null)
        {
            _db.Destinos.Add(new DestinoEntity
            {
                DesGuid = guid,
                DesNombre = nombre.Trim(),
                DesPais = pais.Trim(),
                DesImagenUrl = imagenUrl?.Trim(),
                DesEstado = estado,
                DesFechaIngreso = DateTime.UtcNow,
                DesUsuarioIngreso = usuario,
                DesIpIngreso = ip,
            });
        }
        else
        {
            entity.DesNombre = nombre.Trim();
            entity.DesPais = pais.Trim();
            entity.DesImagenUrl = imagenUrl?.Trim();
            entity.DesEstado = estado;
            entity.DesFechaMod = DateTime.UtcNow;
            entity.DesUsuarioMod = usuario;
            entity.DesIpMod = ip;
            if (estado == 'I')
            {
                entity.DesFechaEliminacion = DateTime.UtcNow;
                entity.DesUsuarioEliminacion = usuario;
                entity.DesIpEliminacion = ip;
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<CategoriaDto>> ListCategoriasActivasAsync(CancellationToken ct = default)
    {
        var rows = await _db.Categorias.AsNoTracking().Where(x => x.CatEstado == 'A').OrderBy(x => x.CatNombre).ToListAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<CategoriaDto?> GetCategoriaAsync(Guid guid, CancellationToken ct = default)
    {
        var x = await _db.Categorias.AsNoTracking().FirstOrDefaultAsync(e => e.CatGuid == guid && e.CatEstado == 'A', ct);
        return x is null ? null : Map(x);
    }

    public async Task UpsertCategoriaAsync(Guid guid, string nombre, Guid? parentGuid, char estado, string usuario, string ip, CancellationToken ct = default)
    {
        if (parentGuid.HasValue)
        {
            var parentOk = await _db.Categorias.AnyAsync(c => c.CatGuid == parentGuid.Value && c.CatEstado == 'A', ct);
            if (!parentOk)
                throw new InvalidOperationException($"Categoría padre {parentGuid} no encontrada.");
        }

        var entity = await _db.Categorias.FirstOrDefaultAsync(x => x.CatGuid == guid, ct);
        if (entity is null)
        {
            _db.Categorias.Add(new CategoriaEntity
            {
                CatGuid = guid,
                CatNombre = nombre.Trim(),
                CatParentGuid = parentGuid,
                CatEstado = estado,
                CatFechaIngreso = DateTime.UtcNow,
                CatUsuarioIngreso = usuario,
                CatIpIngreso = ip,
            });
        }
        else
        {
            entity.CatNombre = nombre.Trim();
            entity.CatParentGuid = parentGuid;
            entity.CatEstado = estado;
            entity.CatFechaMod = DateTime.UtcNow;
            entity.CatUsuarioMod = usuario;
            entity.CatIpMod = ip;
            if (estado == 'I')
            {
                entity.CatFechaEliminacion = DateTime.UtcNow;
                entity.CatUsuarioEliminacion = usuario;
                entity.CatIpEliminacion = ip;
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<IdiomaDto>> ListIdiomasActivosAsync(CancellationToken ct = default)
    {
        var rows = await _db.Idiomas.AsNoTracking().Where(x => x.IdEstado == 'A').OrderBy(x => x.IdDescripcion).ToListAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<IdiomaDto?> GetIdiomaAsync(Guid guid, CancellationToken ct = default)
    {
        var x = await _db.Idiomas.AsNoTracking().FirstOrDefaultAsync(e => e.IdGuid == guid && e.IdEstado == 'A', ct);
        return x is null ? null : Map(x);
    }

    public async Task<bool> IdiomaDescripcionExisteAsync(string descripcion, Guid? excluirGuid, CancellationToken ct = default)
    {
        var n = descripcion.Trim().ToUpperInvariant();
        return await _db.Idiomas.AsNoTracking()
            .AnyAsync(i => i.IdDescripcion.ToUpper() == n && (!excluirGuid.HasValue || i.IdGuid != excluirGuid.Value), ct);
    }

    public async Task UpsertIdiomaAsync(Guid guid, string descripcion, char estado, string usuario, string ip, CancellationToken ct = default)
    {
        var entity = await _db.Idiomas.FirstOrDefaultAsync(x => x.IdGuid == guid, ct);
        if (entity is null)
        {
            _db.Idiomas.Add(new IdiomaEntity
            {
                IdGuid = guid,
                IdDescripcion = descripcion.Trim(),
                IdEstado = estado,
                IdFechaIngreso = DateTime.UtcNow,
                IdUsuarioIngreso = usuario,
                IdIpIngreso = ip,
            });
        }
        else
        {
            entity.IdDescripcion = descripcion.Trim();
            entity.IdEstado = estado;
            entity.IdFechaMod = DateTime.UtcNow;
            entity.IdUsuarioMod = usuario;
            entity.IdIpMod = ip;
            if (estado == 'I')
            {
                entity.IdFechaEliminacion = DateTime.UtcNow;
                entity.IdUsuarioEliminacion = usuario;
                entity.IdIpEliminacion = ip;
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<IncluyeDto>> ListIncluyeActivosAsync(CancellationToken ct = default)
    {
        var rows = await _db.Incluyes.AsNoTracking().Where(x => x.IncEstado == 'A').OrderBy(x => x.IncDescripcion).ToListAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<IncluyeDto?> GetIncluyeAsync(Guid guid, CancellationToken ct = default)
    {
        var x = await _db.Incluyes.AsNoTracking().FirstOrDefaultAsync(e => e.IncGuid == guid && e.IncEstado == 'A', ct);
        return x is null ? null : Map(x);
    }

    public async Task UpsertIncluyeAsync(Guid guid, string descripcion, char estado, CancellationToken ct = default)
    {
        var entity = await _db.Incluyes.FirstOrDefaultAsync(x => x.IncGuid == guid, ct);
        if (entity is null)
            _db.Incluyes.Add(new IncluyeEntity { IncGuid = guid, IncDescripcion = descripcion.Trim(), IncEstado = estado });
        else
        {
            entity.IncDescripcion = descripcion.Trim();
            entity.IncEstado = estado;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ImagenDto>> ListImagenesActivasAsync(CancellationToken ct = default)
    {
        var rows = await _db.Imagenes.AsNoTracking().Where(x => x.ImgEstado == 'A').OrderByDescending(x => x.ImgFechaIngreso).ToListAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<ImagenDto?> GetImagenAsync(Guid guid, CancellationToken ct = default)
    {
        var x = await _db.Imagenes.AsNoTracking().FirstOrDefaultAsync(e => e.ImgGuid == guid && e.ImgEstado == 'A', ct);
        return x is null ? null : Map(x);
    }

    public async Task<bool> ImagenUrlExisteAsync(string url, Guid? excluirGuid, CancellationToken ct = default)
    {
        var n = url.Trim().ToUpperInvariant();
        return await _db.Imagenes.AsNoTracking()
            .AnyAsync(i => i.ImgUrl.ToUpper() == n && (!excluirGuid.HasValue || i.ImgGuid != excluirGuid.Value), ct);
    }

    public async Task UpsertImagenAsync(Guid guid, string url, string? descripcion, char estado, string usuario, string ip, CancellationToken ct = default)
    {
        var entity = await _db.Imagenes.FirstOrDefaultAsync(x => x.ImgGuid == guid, ct);
        if (entity is null)
        {
            _db.Imagenes.Add(new ImagenEntity
            {
                ImgGuid = guid,
                ImgUrl = url.Trim(),
                ImgDescripcion = descripcion?.Trim(),
                ImgEstado = estado,
                ImgFechaIngreso = DateTime.UtcNow,
                ImgUsuarioIngreso = usuario,
                ImgIpIngreso = ip,
            });
        }
        else
        {
            entity.ImgUrl = url.Trim();
            entity.ImgDescripcion = descripcion?.Trim();
            entity.ImgEstado = estado;
            entity.ImgFechaMod = DateTime.UtcNow;
            entity.ImgUsuarioMod = usuario;
            entity.ImgIpMod = ip;
            if (estado == 'I')
            {
                entity.ImgFechaEliminacion = DateTime.UtcNow;
                entity.ImgUsuarioEliminacion = usuario;
                entity.ImgIpEliminacion = ip;
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<DestinoDto>> GetDestinosByGuidsAsync(IEnumerable<Guid> guids, CancellationToken ct = default)
    {
        var set = guids.Distinct().ToHashSet();
        if (set.Count == 0) return Array.Empty<DestinoDto>();
        var rows = await _db.Destinos.AsNoTracking()
            .Where(x => set.Contains(x.DesGuid) && x.DesEstado == 'A').ToListAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<CategoriaDto>> GetCategoriasByGuidsAsync(IEnumerable<Guid> guids, CancellationToken ct = default)
    {
        var set = guids.Distinct().ToHashSet();
        if (set.Count == 0) return Array.Empty<CategoriaDto>();
        var rows = await _db.Categorias.AsNoTracking()
            .Where(x => set.Contains(x.CatGuid) && x.CatEstado == 'A').ToListAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<IdiomaDto>> GetIdiomasByGuidsAsync(IEnumerable<Guid> guids, CancellationToken ct = default)
    {
        var set = guids.Distinct().ToHashSet();
        if (set.Count == 0) return Array.Empty<IdiomaDto>();
        var rows = await _db.Idiomas.AsNoTracking()
            .Where(x => set.Contains(x.IdGuid) && x.IdEstado == 'A').ToListAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<IncluyeDto>> GetIncluyeByGuidsAsync(IEnumerable<Guid> guids, CancellationToken ct = default)
    {
        var set = guids.Distinct().ToHashSet();
        if (set.Count == 0) return Array.Empty<IncluyeDto>();
        var rows = await _db.Incluyes.AsNoTracking()
            .Where(x => set.Contains(x.IncGuid) && x.IncEstado == 'A').ToListAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ImagenDto>> GetImagenesByGuidsAsync(IEnumerable<Guid> guids, CancellationToken ct = default)
    {
        var set = guids.Distinct().ToHashSet();
        if (set.Count == 0) return Array.Empty<ImagenDto>();
        var rows = await _db.Imagenes.AsNoTracking()
            .Where(x => set.Contains(x.ImgGuid) && x.ImgEstado == 'A').ToListAsync(ct);
        return rows.Select(Map).ToList();
    }
}
