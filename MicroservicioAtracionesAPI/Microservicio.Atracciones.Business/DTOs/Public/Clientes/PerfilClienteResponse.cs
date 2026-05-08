namespace Microservicio.Atracciones.Business.DTOs.Public.Clientes
{
    public class PerfilClienteResponse
    {
        public Guid CliGuid { get; set; }
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string TipoIdentificacion { get; set; } = string.Empty;
        public string NumeroIdentificacion { get; set; } = string.Empty;
    }
}
