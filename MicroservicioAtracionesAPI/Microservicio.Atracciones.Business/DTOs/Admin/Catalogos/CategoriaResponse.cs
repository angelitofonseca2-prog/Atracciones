namespace Microservicio.Atracciones.Business.DTOs.Admin.Catalogos
{
    public class CategoriaResponse
    {
        public string CatGuid { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? ParentGuid { get; set; }
        public string? ParentNombre { get; set; }
    }
}