using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Atracciones.Platform.BuildingBlocks.Kestrel;

/// <summary>
/// REST en HTTP/1.1 y gRPC (h2c) en HTTP/2 requieren listeners distintos; en un solo puerto
/// <c>Http1AndHttp2</c> sin TLS, Kestrel responde <c>HTTP_1_1_REQUIRED</c> a clientes gRPC.
/// </summary>
public static class KestrelGrpcRestPorts
{
    public const int DefaultHttpPort = 8080;
    public const int DefaultGrpcPort = 8081;

    public static void Configure(WebApplicationBuilder builder)
    {
        var httpPort = ResolvePort("HTTP_PORT", DefaultHttpPort);
        var grpcPort = ResolvePort("GRPC_PORT", DefaultGrpcPort);

        builder.WebHost.UseSetting(Microsoft.AspNetCore.Hosting.WebHostDefaults.PreferHostingUrlsKey, "false");
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(httpPort, lo => lo.Protocols = HttpProtocols.Http1);
            options.ListenAnyIP(grpcPort, lo => lo.Protocols = HttpProtocols.Http2);
        });
    }

    private static int ResolvePort(string envVar, int fallback) =>
        int.TryParse(Environment.GetEnvironmentVariable(envVar), out var p) && p is > 0 and < 65536
            ? p
            : fallback;
}
