using Microservicio.Atracciones.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Atracciones.Api.Extensions;

public static class MigrateDatabaseExtension
{
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AtraccionesDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseMigration");

        try
        {
            var pending = await db.Database.GetPendingMigrationsAsync();
            if (pending.Any())
            {
                logger.LogInformation("Aplicando {Count} migraci\u00f3n(es) pendiente(s)...", pending.Count());
                await db.Database.MigrateAsync();
                logger.LogInformation("Migraciones aplicadas correctamente.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al aplicar migraciones EF Core. Verifique la conexi\u00f3n a la base de datos.");
            throw;
        }
    }
}
