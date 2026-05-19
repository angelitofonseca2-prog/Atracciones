namespace Atracciones.Platform.BuildingBlocks.Grpc;

/// <summary>Valores por defecto para clientes gRPC (timeout de conexión alineado con AGENTS.md).</summary>
public static class GrpcClientDefaults
{
    public const int ConnectTimeoutSeconds = 2;
    public const int CallDeadlineSeconds = 2;

    public static void ConfigureHandler(SocketsHttpHandler handler)
    {
        handler.EnableMultipleHttp2Connections = true;
        handler.ConnectTimeout = TimeSpan.FromSeconds(ConnectTimeoutSeconds);
        handler.PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5);
    }
}
