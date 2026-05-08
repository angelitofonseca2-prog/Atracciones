
namespace Microservicio.Atracciones.DataManagement.Models.Catalogos
{
    public class CategoriaDataModel
    {
        public int CatId { get; set; }
        public Guid CatGuid { get; set; }
        public int? CatParentId { get; set; }
        public string CatNombre { get; set; } = string.Empty;
        public char CatEstado { get; set; }
        public Guid? ParentGuid { get; set; }
        public string? ParentNombre { get; set; }

        // Hijos (para construir el árbol tipo/subtipo en filtros)
        public IReadOnlyList<CategoriaDataModel> Hijos { get; set; } = new List<CategoriaDataModel>();
    }

}
