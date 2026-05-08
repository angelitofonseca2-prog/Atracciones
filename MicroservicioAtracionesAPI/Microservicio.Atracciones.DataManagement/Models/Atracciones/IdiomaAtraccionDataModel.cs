
namespace Microservicio.Atracciones.DataManagement.Models.Atracciones
{
    public class IdiomaAtraccionDataModel
    {
        public int IdId { get; set; }
        public int AtId { get; set; }
        public char IaEstado { get; set; }
        public string IaUsuarioIngreso { get; set; } = string.Empty;
        public string IaIpIngreso { get; set; } = string.Empty;
    }
}
