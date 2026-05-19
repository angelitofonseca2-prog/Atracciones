using System.Net.Http;
using Atracciones.MsOrquestador.Business.Options;
using Atracciones.Platform.BuildingBlocks.Grpc;
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
        Identidad = CreateChannel(o.Identidad);
        Clientes = CreateChannel(o.Clientes);
        Atracciones = CreateChannel(o.Atracciones);
        Reservas = CreateChannel(o.Reservas);
        Facturacion = CreateChannel(o.Facturacion);
        Auditoria = CreateChannel(o.Auditoria);
    }

    private static GrpcChannel CreateChannel(string address)
    {
        var url = GrpcBaseUrlNormalizer.NormalizeGrpc(address).TrimEnd('/');
        var handler = new SocketsHttpHandler();
        GrpcClientDefaults.ConfigureHandler(handler);

        return GrpcChannel.ForAddress(url, new GrpcChannelOptions
        {
            HttpHandler = handler,
            DisposeHttpClient = true,
        });
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
