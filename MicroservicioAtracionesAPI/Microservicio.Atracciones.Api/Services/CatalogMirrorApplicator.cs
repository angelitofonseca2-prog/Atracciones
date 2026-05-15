using Microservicio.Atracciones.Api.Models.Integration;
using Microservicio.Atracciones.DataAccess.Context;
using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Atracciones.Api.Services;

public interface ICatalogMirrorApplicator
{
    Task ApplyAsync(CatalogMirrorIngressPayload batch, CancellationToken cancellationToken = default);
}

/// <summary>Aplica upserts en las tablas atracciones.* desde ms-catalogos (legacy hasta Fase 4).</summary>
public sealed class CatalogMirrorApplicator : ICatalogMirrorApplicator
{
    private const string MirrorUsuario = "ms-catalogos";
    private const string MirrorIp = "127.0.0.1";

    private readonly AtraccionesDbContext _db;

    public CatalogMirrorApplicator(AtraccionesDbContext db) => _db = db;

    public async Task ApplyAsync(CatalogMirrorIngressPayload batch, CancellationToken cancellationToken = default)
    {
        if (batch.Destinos is { Count: > 0 })
            await ApplyDestinosAsync(batch.Destinos, cancellationToken);

        if (batch.Categorias is { Count: > 0 })
            await ApplyCategoriasAsync(batch.Categorias, cancellationToken);

        if (batch.Idiomas is { Count: > 0 })
            await ApplyIdiomasAsync(batch.Idiomas, cancellationToken);

        if (batch.Incluye is { Count: > 0 })
            await ApplyIncluyeAsync(batch.Incluye, cancellationToken);

        if (batch.Imagenes is { Count: > 0 })
            await ApplyImagenesAsync(batch.Imagenes, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyDestinosAsync(List<DestinoMirrorIngress> rows, CancellationToken ct)
    {
        foreach (var row in rows)
        {
            var entity = await _db.Destinos.FirstOrDefaultAsync(d => d.DesGuid == row.DesGuid, ct);
            if (entity is null)
            {
                _db.Destinos.Add(new DestinoEntity
                {
                    DesGuid = row.DesGuid,
                    DesNombre = row.Nombre,
                    DesPais = row.Pais,
                    DesImagenUrl = row.ImagenUrl,
                    DesEstado = row.Estado,
                    DesFechaIngreso = DateTime.UtcNow,
                    DesUsuarioIngreso = MirrorUsuario,
                    DesIpIngreso = MirrorIp,
                });
                continue;
            }

            entity.DesNombre = row.Nombre;
            entity.DesPais = row.Pais;
            entity.DesImagenUrl = row.ImagenUrl;
            entity.DesEstado = row.Estado;
            entity.DesFechaMod = DateTime.UtcNow;
            entity.DesUsuarioMod = MirrorUsuario;
            entity.DesIpMod = MirrorIp;
            if (row.Estado == 'I')
            {
                entity.DesFechaEliminacion = DateTime.UtcNow;
                entity.DesUsuarioEliminacion = MirrorUsuario;
                entity.DesIpEliminacion = MirrorIp;
            }
            else
            {
                entity.DesFechaEliminacion = null;
                entity.DesUsuarioEliminacion = null;
                entity.DesIpEliminacion = null;
            }
        }
    }

    private async Task ApplyCategoriasAsync(List<CategoriaMirrorIngress> rows, CancellationToken ct)
    {
        var remaining = new List<CategoriaMirrorIngress>(rows);
        while (remaining.Count > 0)
        {
            var progressed = false;
            for (var i = remaining.Count - 1; i >= 0; i--)
            {
                var row = remaining[i];
                if (row.ParentGuid.HasValue)
                {
                    var parentOk = await _db.Categorias.AnyAsync(c => c.CatGuid == row.ParentGuid.Value, ct);
                    if (!parentOk)
                        continue;
                }

                await UpsertCategoriaAsync(row, ct);
                remaining.RemoveAt(i);
                progressed = true;
            }

            if (!progressed)
                throw new InvalidOperationException(
                    "Mirror catálogo: no se pudieron aplicar categorías (padres inexistentes o ciclo).");
        }
    }

    private async Task UpsertCategoriaAsync(CategoriaMirrorIngress row, CancellationToken ct)
    {
        int? parentId = null;
        if (row.ParentGuid.HasValue)
        {
            parentId = await _db.Categorias.AsNoTracking()
                .Where(c => c.CatGuid == row.ParentGuid.Value)
                .Select(c => (int?)c.CatId)
                .FirstOrDefaultAsync(ct);
        }

        var entity = await _db.Categorias.FirstOrDefaultAsync(c => c.CatGuid == row.CatGuid, ct);
        if (entity is null)
        {
            _db.Categorias.Add(new CategoriaEntity
            {
                CatGuid = row.CatGuid,
                CatNombre = row.Nombre,
                CatParentId = parentId,
                CatEstado = row.Estado,
                CatFechaIngreso = DateTime.UtcNow,
                CatUsuarioIngreso = MirrorUsuario,
                CatIpIngreso = MirrorIp,
            });
            return;
        }

        entity.CatNombre = row.Nombre;
        entity.CatParentId = parentId;
        entity.CatEstado = row.Estado;
        entity.CatFechaMod = DateTime.UtcNow;
        entity.CatUsuarioMod = MirrorUsuario;
        entity.CatIpMod = MirrorIp;
        if (row.Estado == 'I')
        {
            entity.CatFechaEliminacion = DateTime.UtcNow;
            entity.CatUsuarioEliminacion = MirrorUsuario;
            entity.CatIpEliminacion = MirrorIp;
        }
        else
        {
            entity.CatFechaEliminacion = null;
            entity.CatUsuarioEliminacion = null;
            entity.CatIpEliminacion = null;
        }
    }

    private async Task ApplyIdiomasAsync(List<IdiomaMirrorIngress> rows, CancellationToken ct)
    {
        foreach (var row in rows)
        {
            var entity = await _db.Idiomas.FirstOrDefaultAsync(i => i.IdGuid == row.IdGuid, ct);
            if (entity is null)
            {
                _db.Idiomas.Add(new IdiomaEntity
                {
                    IdGuid = row.IdGuid,
                    IdDescripcion = row.Descripcion,
                    IdEstado = row.Estado,
                    IdFechaIngreso = DateTime.UtcNow,
                    IdUsuarioIngreso = MirrorUsuario,
                    IdIpIngreso = MirrorIp,
                });
                continue;
            }

            entity.IdDescripcion = row.Descripcion;
            entity.IdEstado = row.Estado;
            entity.IdFechaMod = DateTime.UtcNow;
            entity.IdUsuarioMod = MirrorUsuario;
            entity.IdIpMod = MirrorIp;
            if (row.Estado == 'I')
            {
                entity.IdFechaEliminacion = DateTime.UtcNow;
                entity.IdUsuarioEliminacion = MirrorUsuario;
                entity.IdIpEliminacion = MirrorIp;
            }
            else
            {
                entity.IdFechaEliminacion = null;
                entity.IdUsuarioEliminacion = null;
                entity.IdIpEliminacion = null;
            }
        }
    }

    private async Task ApplyIncluyeAsync(List<IncluyeMirrorIngress> rows, CancellationToken ct)
    {
        foreach (var row in rows)
        {
            var entity = await _db.Incluyes.FirstOrDefaultAsync(i => i.IncGuid == row.IncGuid, ct);
            if (entity is null)
            {
                _db.Incluyes.Add(new IncluyeEntity
                {
                    IncGuid = row.IncGuid,
                    IncDescripcion = row.Descripcion,
                    IncEstado = row.Estado,
                });
                continue;
            }

            entity.IncDescripcion = row.Descripcion;
            entity.IncEstado = row.Estado;
        }
    }

    private async Task ApplyImagenesAsync(List<ImagenMirrorIngress> rows, CancellationToken ct)
    {
        foreach (var row in rows)
        {
            var fechaIngreso = row.FechaIngreso == default ? DateTime.UtcNow : row.FechaIngreso;
            var entity = await _db.Imagenes.FirstOrDefaultAsync(i => i.ImgGuid == row.ImgGuid, ct);
            if (entity is null)
            {
                _db.Imagenes.Add(new ImagenEntity
                {
                    ImgGuid = row.ImgGuid,
                    ImgUrl = row.Url,
                    ImgDescripcion = row.Descripcion,
                    ImgEstado = row.Estado,
                    ImgFechaIngreso = fechaIngreso,
                    ImgUsuarioIngreso = MirrorUsuario,
                    ImgIpIngreso = MirrorIp,
                });
                continue;
            }

            entity.ImgUrl = row.Url;
            entity.ImgDescripcion = row.Descripcion;
            entity.ImgEstado = row.Estado;
            entity.ImgFechaMod = DateTime.UtcNow;
            entity.ImgUsuarioMod = MirrorUsuario;
            entity.ImgIpMod = MirrorIp;
            if (row.Estado == 'I')
            {
                entity.ImgFechaEliminacion = DateTime.UtcNow;
                entity.ImgUsuarioEliminacion = MirrorUsuario;
                entity.ImgIpEliminacion = MirrorIp;
            }
            else
            {
                entity.ImgFechaEliminacion = null;
                entity.ImgUsuarioEliminacion = null;
                entity.ImgIpEliminacion = null;
            }
        }
    }
}
