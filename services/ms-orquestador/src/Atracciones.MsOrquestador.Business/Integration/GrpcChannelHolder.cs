using Atracciones.MsOrquestador.Business.Options;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;

namespace Atracciones.MsOrquestador.Business.Integration;

/// <summary>Canales gRPC por destino (vida útil = aplicación).</summary>
public sealed class GrpcChannelHolder : IDisposable
{
    public GrpcChannel Identidad { get; }
    public GrpcChannel Clientes { get; }
    public GrpcChannel Atracciones { get; }
    public GrpcChannel Reservas { get; }
    public GrpcChannel Facturacion { get; }
    public GrpcChannel Auditoria { get; }

    public GrpcChannelHolder(IOptions<GrpcClientsOptions> options)
    {
        var o = options.Value;
        Identidad = GrpcChannel.ForAddress(o.Identidad.TrimEnd('/'));
        Clientes = GrpcChannel.ForAddress(o.Clientes.TrimEnd('/'));
        Atracciones = GrpcChannel.ForAddress(o.Atracciones.TrimEnd('/'));
        Reservas = GrpcChannel.ForAddress(o.Reservas.TrimEnd('/'));
        Facturacion = GrpcChannel.ForAddress(o.Facturacion.TrimEnd('/'));
        Auditoria = GrpcChannel.ForAddress(o.Auditoria.TrimEnd('/'));
    }

    public void Dispose()
    {
        Identidad.Dispose();
        Clientes.Dispose();
        Atracciones.Dispose();
        Reservas.Dispose();
        Facturacion.Dispose();
        Auditoria.Dispose();
    }
}
