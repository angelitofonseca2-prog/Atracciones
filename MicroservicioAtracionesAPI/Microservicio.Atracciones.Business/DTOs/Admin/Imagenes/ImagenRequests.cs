using System.ComponentModel.DataAnnotations;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Imagenes
{
    public class CrearImagenRequest
    {
        [Required]
        [MaxLength(500)]
        [Url]
        public string Url { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }
    }

    public class ActualizarImagenRequest
    {
        [MaxLength(500)]
        [Url]
        public string? Url { get; set; }

        [MaxLength(200)]
        public string? Descripcion { get; set; }
    }

    public class ImagenResponse
    {
        public string ImgGuid { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public char Estado { get; set; }
        public DateTime FechaIngreso { get; set; }
    }
}
