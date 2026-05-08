namespace Microservicio.Atracciones.Api.Models.Common
{
    public class ApiErrorResponse
    {
        public int Status { get; set; }
        public string Error { get; set; } = string.Empty;
        public IList<string> Details { get; set; } = new List<string>();
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        public string Path { get; set; } = string.Empty;
    }
}
