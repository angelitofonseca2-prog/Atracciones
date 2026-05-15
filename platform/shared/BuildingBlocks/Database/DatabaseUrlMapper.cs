namespace Atracciones.BuildingBlocks.Database;

/// <summary>
/// Convierte la variable DATABASE_URL (formato postgres://user:pass@host:port/db)
/// al formato de connection string Npgsql que exige EF Core.
/// Llamar en la primera línea de Program.cs de cada microservicio, antes de
/// WebApplication.CreateBuilder(args).
/// </summary>
public static class DatabaseUrlMapper
{
    /// <summary>
    /// Si <paramref name="targetEnvVar"/> no está definida, intenta leer DATABASE_URL
    /// y convertirla al formato Npgsql, asignando el resultado a <paramref name="targetEnvVar"/>.
    /// </summary>
    /// <param name="targetEnvVar">Nombre de la variable de entorno que espera el servicio,
    /// p.ej. "ConnectionStrings__InventarioDb".</param>
    /// <param name="searchPath">Schema por defecto (Search Path) a añadir al connection string,
    /// si aplica. Null para no incluirlo.</param>
    /// <param name="requireSsl">Si true, añade SSL Mode=Require;Trust Server Certificate=true.
    /// Usar false para postgres.railway.internal (red interna, sin TLS).</param>
    public static void Apply(string targetEnvVar, string? searchPath = null, bool requireSsl = false)
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(targetEnvVar)))
            return;

        var url = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrWhiteSpace(url))
            return;

        var cs = ConvertToNpgsql(url.Trim(), searchPath, requireSsl);
        if (!string.IsNullOrEmpty(cs))
            Environment.SetEnvironmentVariable(targetEnvVar, cs);
    }

    /// <summary>
    /// Aplica el mapper para múltiples variables de entorno con la misma DATABASE_URL.
    /// Útil cuando un servicio usa dos DbContext que apuntan a la misma BD
    /// (p.ej. ms-atracciones: InventarioDb y CatalogosDb).
    /// </summary>
    public static void ApplyToAll(params string[] targetEnvVars)
    {
        foreach (var v in targetEnvVars)
            Apply(v);
    }

    private static string? ConvertToNpgsql(string databaseUrl, string? searchPath, bool requireSsl)
    {
        try
        {
            if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri))
                return null;

            var userInfo = uri.UserInfo.Split(':', 2);
            var user = Uri.UnescapeDataString(userInfo[0]);
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
            if (string.IsNullOrEmpty(database)) database = "postgres";
            var host = uri.Host;
            if (string.IsNullOrEmpty(host)) return null;

            var cs = $"Host={host};Port={port};Database={database};Username={user};Password={password}";

            if (!string.IsNullOrWhiteSpace(searchPath))
                cs += $";Search Path={searchPath}";

            if (requireSsl)
                cs += ";SSL Mode=Require;Trust Server Certificate=true";

            return cs;
        }
        catch
        {
            return null;
        }
    }
}
