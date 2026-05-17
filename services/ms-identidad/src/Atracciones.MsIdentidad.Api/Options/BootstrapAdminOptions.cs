namespace Atracciones.MsIdentidad.Api.Options;

/// <summary>
/// Admin inicial vía variables de entorno (Railway): BootstrapAdmin__Login y BootstrapAdmin__Password.
/// </summary>
public sealed class BootstrapAdminOptions
{
    public const string SectionName = "BootstrapAdmin";

    public string? Login { get; set; }
    public string? Password { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
}
