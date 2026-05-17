using System.Text.RegularExpressions;

namespace Atracciones.MsOrquestador.Business.Integration;

/// <summary>
/// Ajusta URLs para Docker Compose y Railway (REST :8080, gRPC :8081 en red privada).
/// </summary>
public static partial class GrpcBaseUrlNormalizer
{
    public const int HttpPort = 8080;
    public const int GrpcPort = 8081;

    public static string NormalizeGrpc(string? raw) =>
        Normalize(raw, GrpcPort, rewriteLegacyGrpcPort: true);

    public static string NormalizeHttp(string? raw) =>
        Normalize(raw, HttpPort, rewriteLegacyGrpcPort: false);

    private static string Normalize(string? raw, int defaultPort, bool rewriteLegacyGrpcPort)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return raw?.Trim() ?? string.Empty;

        var trimmed = raw.Trim().TrimEnd('/');
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            return trimmed;

        if (IsRailwayPublicEdge(uri))
            return ToPrivateBase(uri.Host, defaultPort);

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            return trimmed;

        if (IsRailwayPrivateHost(uri.Host))
        {
            if (uri.IsDefaultPort || (rewriteLegacyGrpcPort && uri.Port == HttpPort))
                return $"{Uri.UriSchemeHttp}://{uri.Host}:{defaultPort}";

            return trimmed;
        }

        return trimmed;
    }

    private static bool IsRailwayPublicEdge(Uri uri) =>
        string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
        && uri.Host.EndsWith(".up.railway.app", StringComparison.OrdinalIgnoreCase);

    private static string ToPrivateBase(string publicHost, int port)
    {
        var match = RailwayPublicHostRegex().Match(publicHost);
        var name = match.Success
            ? match.Groups["name"].Value
            : publicHost.Split('.')[0];

        return $"{Uri.UriSchemeHttp}://{name}.railway.internal:{port}";
    }

    private static bool IsRailwayPrivateHost(string host) =>
        host.Equals("railway.internal", StringComparison.OrdinalIgnoreCase)
        || host.EndsWith(".railway.internal", StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex(
        @"^(?<name>[a-z0-9-]+?)(?:-production(?:-[a-z0-9]+)?)?\.up\.railway\.app$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RailwayPublicHostRegex();
}
