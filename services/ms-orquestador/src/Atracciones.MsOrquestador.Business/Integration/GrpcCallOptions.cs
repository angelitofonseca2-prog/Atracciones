using Atracciones.Platform.BuildingBlocks.Grpc;
using Grpc.Core;

namespace Atracciones.MsOrquestador.Business.Integration;

internal static class GrpcCallOptions
{
    public static CallOptions ForDependency(CancellationToken cancellationToken = default)
        => new(
            deadline: DateTime.UtcNow.AddSeconds(GrpcClientDefaults.CallDeadlineSeconds),
            cancellationToken: cancellationToken);
}
