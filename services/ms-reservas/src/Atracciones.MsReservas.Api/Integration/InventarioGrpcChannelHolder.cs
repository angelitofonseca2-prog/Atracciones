using System.Net.Http;
using Atracciones.MsReservas.Api.Options;
using Atracciones.Platform.BuildingBlocks.Grpc;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;

namespace Atracciones.MsReservas.Api.Integration;

public sealed class InventarioGrpcChannelHolder : IDisposable
{
    public GrpcChannel Atracciones { get; }

    public InventarioGrpcChannelHolder(IOptions<GrpcClientsOptions> options)
    {
        var url = GrpcBaseUrlNormalizer.NormalizeGrpc(options.Value.Atracciones).TrimEnd('/');
        var handler = new SocketsHttpHandler();
        GrpcClientDefaults.ConfigureHandler(handler);
        Atracciones = GrpcChannel.ForAddress(url, new GrpcChannelOptions
        {
            HttpHandler = handler,
            DisposeHttpClient = true,
        });
    }

    public void Dispose() => Atracciones.Dispose();
}
