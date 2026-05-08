namespace Microservicio.Atracciones.Api.Models.Settings
{
    public class CacheSettings
    {
        public int ListadoTtlSegundos { get; set; } = 300;
        public int DetalleTtlSegundos { get; set; } = 300;
        public int FiltrosTtlSegundos { get; set; } = 21600;
    }
}
