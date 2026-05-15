using Atracciones.MsReservas.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atracciones.MsReservas.DataAccess;

public sealed class CrmDbContextFactory : IDesignTimeDbContextFactory<CrmDbContext>
{
    public CrmDbContext CreateDbContext(string[] args)
    {
        var cs =
            Environment.GetEnvironmentVariable("VENTAS_DESIGN_CONNECTION")
            ?? "Host=localhost;Port=5437;Database=ventas;Username=ventas;Password=ventas";

        var o = new DbContextOptionsBuilder<CrmDbContext>();
        o.UseNpgsql(cs, npgsql =>
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "crm"));
        return new CrmDbContext(o.Options);
    }
}
