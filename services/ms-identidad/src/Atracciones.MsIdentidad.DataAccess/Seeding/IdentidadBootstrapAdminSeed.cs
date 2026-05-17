using Atracciones.MsIdentidad.DataAccess.Context;
using Atracciones.MsIdentidad.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsIdentidad.DataAccess.Seeding;

/// <summary>
/// Crea o repara un usuario ADMIN (hash BCrypt correcto). Usado con BootstrapAdmin__* en Railway.
/// </summary>
public static class IdentidadBootstrapAdminSeed
{
    public static async Task EnsureAsync(
        IdentidadDbContext db,
        string login,
        string passwordHash,
        CancellationToken ct = default)
    {
        var normalized = login.Trim();
        if (normalized.Length == 0)
            return;

        var adminRol = await db.Roles.AsNoTracking().FirstOrDefaultAsync(
            r => r.RolEstado == 'A' && r.RolDescripcion.ToUpper() == "ADMIN",
            ct);
        if (adminRol is null)
            return;

        var usuario = await db.Usuarios
            .Include(u => u.UsuarioRoles)
            .FirstOrDefaultAsync(u => u.UsuLogin == normalized, ct);

        if (usuario is null)
        {
            usuario = new UsuarioEntity
            {
                UsuLogin = normalized,
                UsuPasswordHash = passwordHash,
                UsuUsuarioRegistro = "bootstrap-admin",
                UsuIpRegistro = "127.0.0.1",
                UsuEstado = 'A',
                CliId = null,
            };
            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            usuario.UsuPasswordHash = passwordHash;
            usuario.UsuEstado = 'A';
            await db.SaveChangesAsync(ct);
        }

        var tieneAdmin = usuario.UsuarioRoles.Any(ur =>
            ur.RolId == adminRol.RolId && ur.UsuRolEstado == 'A');
        if (!tieneAdmin)
        {
            db.UsuarioRoles.Add(new UsuarioRolEntity
            {
                UsuId = usuario.UsuId,
                RolId = adminRol.RolId,
                UsuRolEstado = 'A',
            });
            await db.SaveChangesAsync(ct);
        }
    }
}
