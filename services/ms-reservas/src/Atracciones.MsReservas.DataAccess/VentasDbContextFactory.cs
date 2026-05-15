using Atracciones.MsReservas.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atracciones.MsReservas.DataAccess;

public sealed class VentasDbContextFactory : IDesignTimeDbContextFactory<VentasDbContext>
{
    public VentasDbContext CreateDbContext(string[] args)
    {
        var cs =
            Environment.GetEnvironmentVariable("VENTAS_DESIGN_CONNECTION")
            ?? "Host=localhost;Port=5437;Database=ventas;Username=ventas;Password=ventas";

        var o = new DbContextOptionsBuilder<VentasDbContext>();
        o.UseNpgsql(cs, npgsql =>
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ventas"));
        return new VentasDbContext(o.Options);
    }
}
