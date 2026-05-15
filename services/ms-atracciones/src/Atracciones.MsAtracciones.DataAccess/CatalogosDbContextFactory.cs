using Atracciones.MsAtracciones.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atracciones.MsAtracciones.DataAccess;

public sealed class CatalogosDbContextFactory : IDesignTimeDbContextFactory<CatalogosDbContext>
{
    public CatalogosDbContext CreateDbContext(string[] args)
    {
        var cs =
            Environment.GetEnvironmentVariable("INVENTARIO_DESIGN_CONNECTION")
            ?? "Host=localhost;Port=5436;Database=inventario;Username=inventario;Password=inventario";

        var o = new DbContextOptionsBuilder<CatalogosDbContext>();
        o.UseNpgsql(cs, npgsql =>
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalogos"));
        return new CatalogosDbContext(o.Options);
    }
}
