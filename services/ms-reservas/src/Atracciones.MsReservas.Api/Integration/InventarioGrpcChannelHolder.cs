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
        var raw = GrpcBaseUrlNormalizer.NormalizeGrpc(options.Value.Atracciones).TrimEnd('/');
        // Cuando la variable GrpcClients:Atracciones no está configurada, usar un placeholder
        // que no lanza UriFormatException en el constructor. Los RPC fallarán en runtime
        // (best-effort: ya están envueltos en try-catch en LiberarCupoBestEffortAsync).
        var url = string.IsNullOrWhiteSpace(raw) ? "http://localhost:5401" : raw;
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
