namespace Atracciones.MsAtracciones.Api.Configuration;

internal static class DatabaseUrlMapper
{
    internal static void Apply(string targetEnvVar, string? searchPath = null)
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(targetEnvVar))) return;
        var url = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrWhiteSpace(url)) return;
        var cs = Convert(url.Trim(), searchPath);
        if (cs is not null) Environment.SetEnvironmentVariable(targetEnvVar, cs);
    }

    private static string? Convert(string url, string? searchPath)
    {
        try
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return null;
            var ui = uri.UserInfo.Split(':', 2);
            var user = Uri.UnescapeDataString(ui[0]);
            var pwd  = ui.Length > 1 ? Uri.UnescapeDataString(ui[1]) : string.Empty;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var db   = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
            if (string.IsNullOrEmpty(db)) db = "postgres";
            if (string.IsNullOrEmpty(uri.Host)) return null;
            var cs = $"Host={uri.Host};Port={port};Database={db};Username={user};Password={pwd}";
            if (!string.IsNullOrWhiteSpace(searchPath)) cs += $";Search Path={searchPath}";
            return cs;
        }
        catch { return null; }
    }
}
