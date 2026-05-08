using System.ComponentModel.DataAnnotations;

namespace Microservicio.Atracciones.Business.DTOs.Public.Atracciones
{
    public class AtraccionFiltroRequest
    {
        public string? Ciudad { get; set; }
        public string? Tipo { get; set; }   // cat_guid raíz
        public string? Subtipo { get; set; }   // cat_guid hijo
        public string? Etiqueta { get; set; }
        public string? Idioma { get; set; }
        public decimal? CalificacionMin { get; set; }
        public string? Horario { get; set; }   // "05:00-12:00" | "12:00-18:00" | "18:00-05:00"
        public bool? Disponible { get; set; }

        [RegularExpression("trending|lowest_price|highest_weighted_rating",
            ErrorMessage = "ordenar_por debe ser: trending, lowest_price o highest_weighted_rating.")]
        public string OrdenarPor { get; set; } = "trending";

        [Range(1, int.MaxValue, ErrorMessage = "page debe ser mayor a 0.")]
        public int Page { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "limit debe estar entre 1 y 50.")]
        public int Limit { get; set; } = 10;
    }

}
