using System.ComponentModel.DataAnnotations;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Catalogos
{
    public class CrearCategoriaRequest
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public Guid? ParentGuid { get; set; }
    }

    public class ActualizarCategoriaRequest
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public Guid? ParentGuid { get; set; }
    }

    public class CrearIdiomaRequest
    {
        [Required]
        [MaxLength(80)]
        public string Descripcion { get; set; } = string.Empty;
    }

    public class ActualizarIdiomaRequest
    {
        [Required]
        [MaxLength(80)]
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CrearIncluyeRequest
    {
        [Required]
        [MaxLength(200)]
        public string Descripcion { get; set; } = string.Empty;
    }

    public class ActualizarIncluyeRequest
    {
        [Required]
        [MaxLength(200)]
        public string Descripcion { get; set; } = string.Empty;
    }
}
