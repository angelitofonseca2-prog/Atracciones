
namespace Microservicio.Atracciones.DataManagement.Models.Atracciones
{
    public class CategoriaAtraccionDataModel
    {
        public int CatId { get; set; }
        public int AtId { get; set; }
        public char CaEstado { get; set; }
        public string CaUsuarioIngreso { get; set; } = string.Empty;
        public string CaIpIngreso { get; set; } = string.Empty;
    }
}
