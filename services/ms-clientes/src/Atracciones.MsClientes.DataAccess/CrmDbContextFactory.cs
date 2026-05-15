using Atracciones.MsClientes.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atracciones.MsClientes.DataAccess;

/// <summary>Solo diseño (dotnet ef): evita ejecutar JWKS / Program completo.</summary>
public sealed class CrmDbContextFactory : IDesignTimeDbContextFactory<CrmDbContext>
{
    public CrmDbContext CreateDbContext(string[] args)
    {
        var cs =
            Environment.GetEnvironmentVariable("CRM_DESIGN_CONNECTION")
            ?? "Host=localhost;Port=5434;Database=crm;Username=crm;Password=crm";

        var o = new DbContextOptionsBuilder<CrmDbContext>();
        o.UseNpgsql(cs, npgsql =>
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "crm"));
        return new CrmDbContext(o.Options);
    }
}
