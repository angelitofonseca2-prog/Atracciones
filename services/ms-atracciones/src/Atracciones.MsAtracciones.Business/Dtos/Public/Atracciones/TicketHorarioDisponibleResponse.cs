namespace Atracciones.MsAtracciones.Business.Dtos.Public.Atracciones;

/// <summary>Ticket vendible en un horario concreto (disponibilidad del slot).</summary>
public class TicketHorarioDisponibleResponse
{
    public string HorGuid { get; set; } = string.Empty;
    public string TckGuid { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Moneda { get; set; } = "USD";
    public int CuposDisponibles { get; set; }
    public bool Disponible { get; set; }
}
