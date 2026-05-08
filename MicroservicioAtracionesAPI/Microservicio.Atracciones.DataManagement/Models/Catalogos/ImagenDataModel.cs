
namespace Microservicio.Atracciones.DataManagement.Models.Catalogos
{
    public class ImagenDataModel
    {
        public int ImgId { get; set; }
        public Guid ImgGuid { get; set; }
        public string ImgUrl { get; set; } = string.Empty;
        public string? ImgDescripcion { get; set; }
        public char ImgEstado { get; set; }
        public DateTime ImgFechaIngreso { get; set; }
    }

}
