namespace Atracciones.MsOrquestador.Business.Integration;

/// <summary>
/// Railway Private DNS (<c>*.railway.internal</c>) sin puerto explícito resuelve al 80 por defecto en HTTP,
/// pero los contenedores .NET escuchan en <c>8080</c> (ASPNETCORE_URLS). Sin esto, gRPC falla con
/// "Error connecting to subchannel". Si el puerto ya viene en la URL (p. ej. gRPC dedicado :9090 en Docker Compose), no se modifica.
/// </summary>
public static class GrpcBaseUrlNormalizer
{
    public static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return raw?.Trim() ?? string.Empty;

        var trimmed = raw.Trim().TrimEnd('/');
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            return trimmed;

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            return trimmed;

        if (!IsRailwayPrivateHost(uri.Host))
            return trimmed;

        if (!uri.IsDefaultPort)
            return trimmed;

        return $"{Uri.UriSchemeHttp}://{uri.Host}:8080";
    }

    private static bool IsRailwayPrivateHost(string host) =>
        host.Equals("railway.internal", StringComparison.OrdinalIgnoreCase)
        || host.EndsWith(".railway.internal", StringComparison.OrdinalIgnoreCase);
}
