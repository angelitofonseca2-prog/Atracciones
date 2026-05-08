
namespace Microservicio.Atracciones.DataManagement.Models.Atracciones
{
    public class AtraccionIncluyeDataModel
    {
        public int IncId { get; set; }
        public int AtId { get; set; }
        public char AiEstado { get; set; }
        public string AiUsuarioIngreso { get; set; } = string.Empty;
        public string AiIpIngreso { get; set; } = string.Empty;
    }
}
