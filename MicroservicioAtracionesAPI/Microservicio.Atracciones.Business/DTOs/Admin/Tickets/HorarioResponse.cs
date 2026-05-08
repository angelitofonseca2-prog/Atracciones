namespace Microservicio.Atracciones.Business.DTOs.Admin.Tickets
{
    public class HorarioResponse
    {
        public string HorGuid { get; set; } = string.Empty;
        public string TckGuid { get; set; } = string.Empty;
        public string AtraccionGuid { get; set; } = string.Empty;
        public string AtraccionNombre { get; set; } = string.Empty;
        public string TicketTitulo { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;          // "YYYY-MM-DD"
        public string HoraInicio { get; set; } = string.Empty;     // "HH:mm"
        public string? HoraFin { get; set; }
        public int CapacidadMaxima { get; set; }
        public int CuposDisponibles { get; set; }
        public char Estado { get; set; }
        public DateTime FechaIngreso { get; set; }
    }
}
