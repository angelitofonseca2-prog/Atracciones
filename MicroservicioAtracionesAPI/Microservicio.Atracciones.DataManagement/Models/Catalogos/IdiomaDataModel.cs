
namespace Microservicio.Atracciones.DataManagement.Models.Catalogos
{
    public class IdiomaDataModel
    {
        public int IdId { get; set; }
        public Guid IdGuid { get; set; }
        public string IdDescripcion { get; set; } = string.Empty;
        public char IdEstado { get; set; }
    }

}
