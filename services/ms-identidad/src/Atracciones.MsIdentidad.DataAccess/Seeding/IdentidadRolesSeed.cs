using Atracciones.MsIdentidad.DataAccess.Context;
using Atracciones.MsIdentidad.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsIdentidad.DataAccess.Seeding;

public static class IdentidadRolesSeed
{
    private static readonly string[] Esenciales = ["CLIENTE", "ADMIN"];

    public static async Task EnsureAsync(IdentidadDbContext db, CancellationToken ct = default)
    {
        foreach (var desc in Esenciales)
        {
            var exists = await db.Roles.AsNoTracking().AnyAsync(
                r => r.RolEstado == 'A' && r.RolDescripcion.ToUpper() == desc,
                ct);
            if (exists)
                continue;

            db.Roles.Add(new RolEntity
            {
                RolGuid = Guid.NewGuid(),
                RolDescripcion = desc,
                RolEstado = 'A',
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
