using Atracciones.MsAtracciones.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atracciones.MsAtracciones.DataAccess;

public sealed class InventarioDbContextFactory : IDesignTimeDbContextFactory<InventarioDbContext>
{
    public InventarioDbContext CreateDbContext(string[] args)
    {
        var cs = "Host=localhost;Port=5436;Database=inventario;Username=inventario;Password=inventario";
        var o = new DbContextOptionsBuilder<InventarioDbContext>().UseNpgsql(cs, npgsql =>
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "inventario"));
        return new InventarioDbContext(o.Options);
    }
}
