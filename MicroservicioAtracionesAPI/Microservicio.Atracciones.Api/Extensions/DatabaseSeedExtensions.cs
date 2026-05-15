using Microservicio.Atracciones.DataAccess.Context;
using Microservicio.Atracciones.DataAccess.Entities.Seguridad;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Atracciones.Api.Extensions;

/// <summary>
/// Garantiza roles mínimos para registro público (CLIENTE) y panel admin (ADMIN).
/// </summary>
public static class DatabaseSeedExtensions
{
    private static readonly string[] EssentialRoleDescriptions = ["CLIENTE", "ADMIN"];

    public static async Task EnsureEssentialRolesAsync(
        this WebApplication app,
        CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AtraccionesDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseSeed");

        foreach (var desc in EssentialRoleDescriptions)
        {
            var exists = await db.Roles.AsNoTracking().AnyAsync(
                r => r.RolEstado == 'A'
                     && r.RolDescripcion.ToUpper() == desc,
                cancellationToken);

            if (exists)
                continue;

            db.Roles.Add(new RolEntity
            {
                RolGuid = Guid.NewGuid(),
                RolDescripcion = desc,
                RolFechaIngreso = DateTime.UtcNow,
                RolUsuarioIngreso = "system-seed",
                RolIpIngreso = "127.0.0.1",
                RolEstado = 'A',
            });

            logger.LogInformation("Rol semilla insertado: {Rol}", desc);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
