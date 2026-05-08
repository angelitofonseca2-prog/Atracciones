namespace Microservicio.Atracciones.Api.Models.Settings
{
    public class CorsSettings
    {
        public string PolicyName { get; set; } = "AtraccionesPolicy";
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    }
}
