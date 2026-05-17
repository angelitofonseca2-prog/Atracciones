using System.Text.RegularExpressions;

namespace Atracciones.MsOrquestador.Business.Integration;

/// <summary>
/// Ajusta URLs de clientes gRPC para Docker Compose y Railway.
/// <list type="bullet">
/// <item>Private DNS (<c>*.railway.internal</c>) sin puerto → <c>:8080</c> (Kestrel en contenedor).</item>
/// <item>Dominio público <c>https://*.up.railway.app</c> → <c>http://{servicio}.railway.internal:8080</c> (h2c; evita <c>HTTP_1_1_REQUIRED</c> en el edge HTTP/1.1).</item>
/// </list>
/// </summary>
public static partial class GrpcBaseUrlNormalizer
{
    public static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return raw?.Trim() ?? string.Empty;

        var trimmed = raw.Trim().TrimEnd('/');
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            return trimmed;

        if (IsRailwayPublicEdge(uri))
            return ToPrivateH2cBase(uri.Host);

        if (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            && IsRailwayPrivateHost(uri.Host)
            && uri.IsDefaultPort)
            return $"{Uri.UriSchemeHttp}://{uri.Host}:8080";

        if (string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            && IsRailwayPrivateHost(uri.Host))
        {
            var port = uri.IsDefaultPort ? 8080 : uri.Port;
            return $"{Uri.UriSchemeHttp}://{uri.Host}:{port}";
        }

        return trimmed;
    }

    private static bool IsRailwayPublicEdge(Uri uri) =>
        string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
        && uri.Host.EndsWith(".up.railway.app", StringComparison.OrdinalIgnoreCase);

    private static string ToPrivateH2cBase(string publicHost)
    {
        var serviceSlug = RailwayPublicHostRegex().Match(publicHost);
        var name = serviceSlug.Success
            ? serviceSlug.Groups["name"].Value
            : publicHost.Split('.')[0];

        return $"{Uri.UriSchemeHttp}://{name}.railway.internal:8080";
    }

    private static bool IsRailwayPrivateHost(string host) =>
        host.Equals("railway.internal", StringComparison.OrdinalIgnoreCase)
        || host.EndsWith(".railway.internal", StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex(
        @"^(?<name>[a-z0-9-]+?)(?:-production(?:-[a-z0-9]+)?)?\.up\.railway\.app$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RailwayPublicHostRegex();
}
