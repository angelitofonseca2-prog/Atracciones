
namespace Microservicio.Atracciones.Business.DTOs.Auth
{
    public class UsuarioAutenticadoDto
    {
        public int UsuId { get; set; }
        public Guid UsuGuid { get; set; }
        public string Login { get; set; } = string.Empty;
        public int? CliId { get; set; } // E-05
        public IList<string> Roles { get; set; } = new List<string>();
    }

}
