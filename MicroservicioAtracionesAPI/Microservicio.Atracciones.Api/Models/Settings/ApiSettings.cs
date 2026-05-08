namespace Microservicio.Atracciones.Api.Models.Settings
{
    public class ApiSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:8080";
        public string NombreSistema { get; set; } = "Microservicio Atracciones";
        public int PaginacionDefault { get; set; } = 10;
        public int PaginacionMaxima { get; set; } = 50;
    }

}
