using Atracciones.MsCatalogos.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atracciones.MsCatalogos.DataAccess;

public sealed class CatalogosDbContextFactory : IDesignTimeDbContextFactory<CatalogosDbContext>
{
    public CatalogosDbContext CreateDbContext(string[] args)
    {
        var cs =
            Environment.GetEnvironmentVariable("CATALOGOS_DESIGN_CONNECTION")
            ?? "Host=localhost;Port=5435;Database=catalogos;Username=catalogos;Password=catalogos";

        var o = new DbContextOptionsBuilder<CatalogosDbContext>();
        o.UseNpgsql(cs, npgsql =>
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalogos"));
        return new CatalogosDbContext(o.Options);
    }
}
