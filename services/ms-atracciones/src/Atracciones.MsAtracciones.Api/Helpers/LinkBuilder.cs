namespace Atracciones.MsAtracciones.Api.Helpers;

public static class LinkBuilder
{
    public static Dictionary<string, string?> ParaListado(
        string baseUrl, string queryString, int page, int limit, int totalPaginas)
    {
        var baseQuery = RemoverParametro(queryString, "page");
        var sep = baseQuery.Length > 0 ? "&" : "?";

        return new Dictionary<string, string?>
        {
            ["self"] = $"{baseUrl}/api/v2/atracciones{(queryString.Length > 0 ? queryString : string.Empty)}",
            ["next"] = page < totalPaginas
                ? $"{baseUrl}/api/v2/atracciones{baseQuery}{sep}page={page + 1}"
                : null,
            ["prev"] = page > 1
                ? $"{baseUrl}/api/v2/atracciones{baseQuery}{sep}page={page - 1}"
                : null,
        };
    }

    private static string RemoverParametro(string queryString, string param)
    {
        if (string.IsNullOrEmpty(queryString)) return string.Empty;

        var parts = queryString.TrimStart('?')
            .Split('&')
            .Where(p => !p.StartsWith($"{param}=", StringComparison.OrdinalIgnoreCase))
            .ToList();

        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}
