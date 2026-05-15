using Atracciones.MsOrquestador.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atracciones.MsOrquestador.DataAccess;

public sealed class OrquestadorDbContextFactory : IDesignTimeDbContextFactory<OrquestadorDbContext>
{
    public OrquestadorDbContext CreateDbContext(string[] args)
    {
        var cs =
            Environment.GetEnvironmentVariable("ORQUESTADOR_DESIGN_CONNECTION")
            ?? "Host=localhost;Port=5438;Database=orquestador;Username=orquestador;Password=orquestador";

        var o = new DbContextOptionsBuilder<OrquestadorDbContext>();
        o.UseNpgsql(cs, npgsql =>
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "orq"));
        return new OrquestadorDbContext(o.Options);
    }
}
