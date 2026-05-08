using Microservicio.Atracciones.Api.Models.Common;

namespace Microservicio.Atracciones.Api.Helpers
{
    public static class SorterFactory
    {
        public static IList<SorterResponse> ObtenerSorters() =>
        [
            new SorterResponse { Name = "Nuestros destacados",  Value = "trending" },
        new SorterResponse { Name = "Precio más bajo",      Value = "lowest_price" },
        new SorterResponse { Name = "Mejores comentarios",  Value = "highest_weighted_rating" }
        ];

        public static SorterResponse ObtenerDefault() =>
            new() { Name = "Nuestros destacados", Value = "trending" };
    }
}
