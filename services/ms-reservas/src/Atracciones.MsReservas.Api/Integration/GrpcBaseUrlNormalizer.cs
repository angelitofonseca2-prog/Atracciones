namespace Atracciones.MsReservas.Api.Integration;

internal static class GrpcBaseUrlNormalizer
{
    public static string NormalizeGrpc(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        var u = url.Trim();
        if (!u.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            && !u.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            u = "http://" + u;
        }

        return u;
    }
}
