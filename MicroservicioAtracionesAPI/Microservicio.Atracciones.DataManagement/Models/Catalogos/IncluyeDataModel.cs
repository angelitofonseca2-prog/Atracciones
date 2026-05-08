
namespace Microservicio.Atracciones.DataManagement.Models.Catalogos
{
    public class IncluyeDataModel
    {
        public int IncId { get; set; }
        public Guid IncGuid { get; set; }
        public string IncDescripcion { get; set; } = string.Empty;
        public char IncEstado { get; set; }
    }
}
