using Atracciones.MsIdentidad.DataAccess.Entities;
using Atracciones.MsIdentidad.DataAccess.Context;
using Atracciones.MsIdentidad.DataManagement.Interfaces;
using Atracciones.MsIdentidad.DataManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsIdentidad.DataAccess.Repositories;

public sealed class IdentidadUsuarioRepository : IIdentidadUsuarioRepository
{
    private readonly IdentidadDbContext _db;

    public IdentidadUsuarioRepository(IdentidadDbContext db) => _db = db;

    public async Task<UsuarioAuthSnapshot?> ObtenerActivoPorLoginAsync(string login, CancellationToken ct = default)
    {
        var u = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UsuLogin == login && x.UsuEstado == 'A', ct);
        return u is null ? null : MapSnapshot(u);
    }

    public async Task<UsuarioAuthSnapshot?> ObtenerActivoPorGuidAsync(Guid usuGuid, CancellationToken ct = default)
    {
        var u = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UsuGuid == usuGuid && x.UsuEstado == 'A', ct);
        return u is null ? null : MapSnapshot(u);
    }

    public async Task<IReadOnlyList<string>> ListarRolesPorUsuIdAsync(int usuId, CancellationToken ct = default)
    {
        return await _db.UsuarioRoles
            .AsNoTracking()
            .Where(x => x.UsuId == usuId && x.UsuRolEstado == 'A')
            .Join(_db.Roles.AsNoTracking(), ur => ur.RolId, r => r.RolId, (_, r) => r.RolDescripcion.ToUpperInvariant())
            .ToListAsync(ct);
    }

    public async Task UpsertEspejoMonolithAsync(MonolithUsuarioEspejoDto dto, CancellationToken ct = default)
    {
        var roles = await ResolverRolesAsync(dto.Roles, ct);
        var usuario = await _db.Usuarios
            .Include(u => u.UsuarioRoles)
            .FirstOrDefaultAsync(u => u.UsuId == dto.UsuId, ct);

        if (usuario is null)
        {
            usuario = new UsuarioEntity
            {
                UsuId = dto.UsuId,
                UsuGuid = dto.UsuGuid,
                UsuLogin = dto.Login,
                UsuPasswordHash = dto.PasswordHash,
                UsuUsuarioRegistro = "monolith-sync",
                UsuIpRegistro = "127.0.0.1",
                UsuEstado = 'A',
                CliId = dto.CliId,
            };
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync(ct);
            await ReemplazarRolesAsync(usuario.UsuId, roles, ct);
            return;
        }

        usuario.UsuGuid = dto.UsuGuid;
        usuario.UsuLogin = dto.Login;
        usuario.UsuPasswordHash = dto.PasswordHash;
        usuario.CliId = dto.CliId;
        usuario.UsuEstado = 'A';
        _db.UsuarioRoles.RemoveRange(usuario.UsuarioRoles);
        await _db.SaveChangesAsync(ct);
        await ReemplazarRolesAsync(usuario.UsuId, roles, ct);
    }

    public async Task<(int usuId, Guid usuGuid)> CrearUsuarioConRolesAsync(NuevoUsuarioDto dto, CancellationToken ct = default)
    {
        if (await _db.Usuarios.AnyAsync(u => u.UsuLogin == dto.Login, ct))
            throw new InvalidOperationException($"Login '{dto.Login}' ya existe.");

        var roles = await ResolverRolesAsync(dto.Roles, ct);
        var usuario = new UsuarioEntity
        {
            UsuGuid = Guid.NewGuid(),
            UsuLogin = dto.Login,
            UsuPasswordHash = dto.PasswordHash,
            UsuUsuarioRegistro = dto.CreadoPor,
            UsuIpRegistro = dto.IpCreador,
            UsuEstado = 'A',
            CliId = null,
        };
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync(ct);
        await ReemplazarRolesAsync(usuario.UsuId, roles, ct);
        return (usuario.UsuId, usuario.UsuGuid);
    }

    public async Task<(IReadOnlyList<UsuarioAdminListItem> Items, int Total)> ListarParaAdminAsync(int page, int limit, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var q = _db.Usuarios
            .AsNoTracking()
            .Include(u => u.UsuarioRoles)
            .ThenInclude(ur => ur.Rol)
            .Where(u => u.UsuEstado != 'I');

        var total = await q.CountAsync(ct);
        var entities = await q
            .OrderBy(u => u.UsuId)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(ct);

        var items = entities.Select(MapAdminList).ToList();
        return (items, total);
    }

    private static UsuarioAdminListItem MapAdminList(UsuarioEntity u)
    {
        var roles = u.UsuarioRoles
            .Where(ur => ur.UsuRolEstado == 'A' && ur.Rol.RolEstado == 'A')
            .Select(ur => ur.Rol.RolDescripcion.ToUpperInvariant())
            .Distinct()
            .ToList();
        return new UsuarioAdminListItem(u.UsuGuid, u.UsuLogin, u.UsuEstado, roles, u.UsuFechaRegistro);
    }

    public async Task<bool> MarcarInactivoPorGuidAsync(Guid usuGuid, CancellationToken ct = default)
    {
        var u = await _db.Usuarios.FirstOrDefaultAsync(x => x.UsuGuid == usuGuid, ct);
        if (u is null) return false;
        u.UsuEstado = 'I';
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static UsuarioAuthSnapshot MapSnapshot(UsuarioEntity u) => new()
    {
        UsuId = u.UsuId,
        UsuGuid = u.UsuGuid,
        Login = u.UsuLogin,
        PasswordHash = u.UsuPasswordHash,
        CliId = u.CliId,
        Estado = u.UsuEstado,
    };

    private async Task<List<RolEntity>> ResolverRolesAsync(IReadOnlyList<string> descripciones, CancellationToken ct)
    {
        var normalizados = descripciones
            .Select(r => r.Trim().ToUpperInvariant())
            .Where(r => r.Length > 0)
            .Distinct()
            .ToList();

        var roles = await _db.Roles
            .Where(r => r.RolEstado == 'A' && normalizados.Contains(r.RolDescripcion.ToUpper()))
            .ToListAsync(ct);

        var faltantes = normalizados.Except(roles.Select(r => r.RolDescripcion.ToUpperInvariant())).ToList();
        if (faltantes.Count > 0)
            throw new InvalidOperationException($"Roles no encontrados: {string.Join(", ", faltantes)}");

        return roles;
    }

    private async Task ReemplazarRolesAsync(int usuId, IReadOnlyList<RolEntity> roles, CancellationToken ct)
    {
        foreach (var rol in roles)
        {
            _db.UsuarioRoles.Add(new UsuarioRolEntity
            {
                UsuId = usuId,
                RolId = rol.RolId,
                UsuRolEstado = 'A',
            });
        }

        await _db.SaveChangesAsync(ct);
    }
}
