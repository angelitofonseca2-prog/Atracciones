using Atracciones.Contracts.Auditoria.V1;
using Atracciones.MsAuditoria.DataManagement.Interfaces;
using Grpc.Core;

namespace Atracciones.MsAuditoria.Api.Grpc;

public sealed class AuditoriaGrpcService : AuditoriaService.AuditoriaServiceBase
{
    private readonly IAuditoriaRepository _repo;
    private readonly ILogger<AuditoriaGrpcService> _logger;

    public AuditoriaGrpcService(IAuditoriaRepository repo, ILogger<AuditoriaGrpcService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public override async Task<RegistrarEventoReply> RegistrarEvento(RegistrarEventoRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Tipo))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "tipo es obligatorio."));

        try
        {
            await _repo.RegistrarEventoAsync(
                request.Tipo,
                request.CorrelationId ?? string.Empty,
                request.PayloadJson ?? "{}",
                context.CancellationToken);
            return new RegistrarEventoReply { Ok = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RegistrarEvento");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}
