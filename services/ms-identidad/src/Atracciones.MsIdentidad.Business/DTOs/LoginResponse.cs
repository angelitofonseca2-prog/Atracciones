namespace Atracciones.MsIdentidad.Business.DTOs;

public sealed class LoginResponse
{
    public string Login { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
    public string Token { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
}
