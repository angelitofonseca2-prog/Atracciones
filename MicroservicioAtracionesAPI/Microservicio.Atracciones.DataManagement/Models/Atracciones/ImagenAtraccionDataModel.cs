
namespace Microservicio.Atracciones.DataManagement.Models.Atracciones
{
    public class ImagenAtraccionDataModel
    {
        public int ImgId { get; set; }
        public int AtId { get; set; }
        public char ImaEstado { get; set; }
        public string ImaUsuarioIngreso { get; set; } = string.Empty;
        public string ImaIpIngreso { get; set; } = string.Empty;
    }
}
