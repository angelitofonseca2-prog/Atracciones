
namespace Microservicio.Atracciones.DataManagement.Models.Atracciones
{
    public class AtraccionFiltroDataModel
    {
        public string? Ciudad { get; set; }
        public string? TextoBusqueda { get; set; }
        public char? Estado { get; set; }
        public Guid? TipoCatGuid { get; set; }
        public Guid? SubtipoCatGuid { get; set; }
        public string? Etiqueta { get; set; }
        public string? Idioma { get; set; }
        public decimal? CalificacionMin { get; set; }
        public string? Horario { get; set; }
        public bool? Disponible { get; set; }
        public string OrdenarPor { get; set; } = "trending";
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }
}
