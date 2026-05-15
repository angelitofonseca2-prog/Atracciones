using System.ComponentModel.DataAnnotations;

namespace Atracciones.MsAtracciones.Business.Dtos.Public.Atracciones;

public class AtraccionFiltroRequest
{
    public string? Ciudad { get; set; }
    public string? Tipo { get; set; }
    public string? Subtipo { get; set; }
    public string? Etiqueta { get; set; }
    public string? Idioma { get; set; }
    public decimal? CalificacionMin { get; set; }
    public string? Horario { get; set; }
    public bool? Disponible { get; set; }

    [RegularExpression("trending|lowest_price|highest_weighted_rating")]
    public string OrdenarPor { get; set; } = "trending";

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 50)]
    public int Limit { get; set; } = 10;
}
