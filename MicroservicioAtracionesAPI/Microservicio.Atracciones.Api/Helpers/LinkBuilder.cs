namespace Microservicio.Atracciones.Api.Helpers
{
    public static class LinkBuilder
    {
        /// <summary>Links de un ítem dentro del listado (self + detalle).</summary>
        public static Dictionary<string, string?> ParaItemListado(string baseUrl, Guid atGuid)
            => new()
            {
                ["self"] = $"{baseUrl}/api/v1/atracciones/{atGuid}",
                ["detalle"] = $"{baseUrl}/api/v1/atracciones/{atGuid}"
            };

        /// <summary>Links de la envoltura del listado (self, next, prev).</summary>
        public static Dictionary<string, string?> ParaListado(
            string baseUrl, string queryString, int page, int limit, int totalPaginas)
        {
            // Reconstruye la query sin el parámetro page para poder añadirlo limpio
            var baseQuery = RemoverParametro(queryString, "page");
            var sep = baseQuery.Length > 0 ? "&" : "?";

            return new Dictionary<string, string?>
            {
                ["self"] = $"{baseUrl}/api/v1/atracciones{(queryString.Length > 0 ? queryString : string.Empty)}",
                ["next"] = page < totalPaginas
                           ? $"{baseUrl}/api/v1/atracciones{baseQuery}{sep}page={page + 1}"
                           : null,
                ["prev"] = page > 1
                           ? $"{baseUrl}/api/v1/atracciones{baseQuery}{sep}page={page - 1}"
                           : null
            };
        }

        /// <summary>Links del detalle de una atracción (self + listado).</summary>
        public static Dictionary<string, string?> ParaDetalle(string baseUrl, Guid atGuid, string? ciudad)
            => new()
            {
                ["self"] = $"{baseUrl}/api/v1/atracciones/{atGuid}",
                ["listado"] = ciudad is not null
                              ? $"{baseUrl}/api/v1/atracciones?ciudad={ciudad}"
                              : $"{baseUrl}/api/v1/atracciones"
            };

        private static string RemoverParametro(string queryString, string param)
        {
            if (string.IsNullOrEmpty(queryString)) return string.Empty;

            var parts = queryString.TrimStart('?')
                                   .Split('&')
                                   .Where(p => !p.StartsWith($"{param}=", StringComparison.OrdinalIgnoreCase))
                                   .ToList();

            return parts.Any() ? "?" + string.Join("&", parts) : string.Empty;
        }
    }
}
