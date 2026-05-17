namespace Atracciones.MsOrquestador.Business.Options;

public sealed class GrpcClientsOptions
{
    public const string SectionName = "GrpcClients";

    public string Identidad { get; set; } = "http://localhost:5101";
    public string IdentidadHttp { get; set; } = "http://localhost:5101";
    /// <summary>Tras fusión con ventas: mismo host/puerto que <see cref="Reservas"/> (ms-reservas).</summary>
    public string Clientes { get; set; } = "http://localhost:5601";
    public string Atracciones { get; set; } = "http://localhost:5401";
    public string Reservas { get; set; } = "http://localhost:5601";
    public string Facturacion { get; set; } = "http://localhost:5701";
    public string Auditoria { get; set; } = "http://localhost:5801";
}
