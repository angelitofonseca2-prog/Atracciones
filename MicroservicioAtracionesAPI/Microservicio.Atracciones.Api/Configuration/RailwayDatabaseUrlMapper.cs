namespace Microservicio.Atracciones.Api.Configuration;

/// <summary>
/// Railway (y similares) suelen exponer <c>DATABASE_URL</c> (postgres://...).
/// Npgsql no acepta <c>Port=0</c>; los placeholders con <c>Port=0000</c> también se interpretan como 0.
/// </summary>
public static class RailwayDatabaseUrlMapper
{
    public static void Apply()
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ConnectionStrings__AtraccionesDb")))
            return;

        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrWhiteSpace(databaseUrl))
            return;

        var cs = BuildNpgsqlConnectionString(databaseUrl.Trim());
        if (!string.IsNullOrEmpty(cs))
            Environment.SetEnvironmentVariable("ConnectionStrings__AtraccionesDb", cs);
    }

    private static string? BuildNpgsqlConnectionString(string databaseUrl)
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
            if (string.IsNullOrEmpty(database))
                database = "postgres";

            var host = uri.Host;
            if (string.IsNullOrEmpty(host))
                return null;

            return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true;Search Path=atracciones";
        }
        catch
        {
            return null;
        }
    }
}
