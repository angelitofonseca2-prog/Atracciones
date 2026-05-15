using Atracciones.MsIdentidad.DataAccess.Context;
using Atracciones.MsIdentidad.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsIdentidad.DataAccess.Seeding;

/// <summary>
/// Crea un usuario con rol ADMIN solo en entornos de desarrollo (Docker/local),
/// para poder abrir /admin/usuarios sin haber ejecutado el ETL.
/// </summary>
public static class IdentidadDevAdminSeed
{
    public const string DefaultLogin = "devadmin";

    public static async Task EnsureAsync(
        IdentidadDbContext db,
        string login,
        string passwordHash,
        CancellationToken ct = default)
    {
        var normalized = login.Trim();
        if (normalized.Length == 0)
            return;

        if (await db.Usuarios.AnyAsync(u => u.UsuLogin == normalized, ct))
            return;

        var adminRol = await db.Roles.AsNoTracking().FirstOrDefaultAsync(
            r => r.RolEstado == 'A' && r.RolDescripcion.ToUpper() == "ADMIN",
            ct);
        if (adminRol is null)
            return;

        var usuario = new UsuarioEntity
        {
            UsuLogin = normalized,
            UsuPasswordHash = passwordHash,
            UsuUsuarioRegistro = "seed-dev",
            UsuIpRegistro = "127.0.0.1",
            UsuEstado = 'A',
            CliId = null,
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(ct);

        db.UsuarioRoles.Add(new UsuarioRolEntity
        {
            UsuId = usuario.UsuId,
            RolId = adminRol.RolId,
            UsuRolEstado = 'A',
        });
        await db.SaveChangesAsync(ct);
    }
}
