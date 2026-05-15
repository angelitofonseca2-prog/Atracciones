namespace Atracciones.MsAtracciones.Business.Dtos.Admin.Atracciones;

public class AtraccionAdminFiltroRequest
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
    public string? Busqueda { get; set; }
}
