using Atracciones.Contracts.Facturacion.V1;
using Atracciones.MsFacturacion.DataManagement.Interfaces;
using Atracciones.MsFacturacion.DataManagement.Models;
using Grpc.Core;

namespace Atracciones.MsFacturacion.Api.Grpc;

public sealed class FacturaGrpcService : FacturaService.FacturaServiceBase
{
    private readonly IFacturaRepository _repo;
    private readonly ILogger<FacturaGrpcService> _logger;

    public FacturaGrpcService(IFacturaRepository repo, ILogger<FacturaGrpcService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public override async Task<FacturaReply> EmitirFactura(EmitirFacturaRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.RevGuid, out var revGuid) || !Guid.TryParse(request.CliGuid, out var cliGuid))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "rev_guid o cli_guid inválido."));

        var datos = request.Datos ?? new EmitirFacturaDatos();
        try
        {
            var dto = new EmitirFacturaInternaDto
            {
                RevGuid = revGuid,
                CliGuid = cliGuid,
                NombreReceptor = datos.NombreReceptor ?? string.Empty,
                CorreoReceptor = datos.CorreoReceptor ?? string.Empty,
                TelefonoReceptor = datos.TelefonoReceptor ?? string.Empty,
                Total = (decimal)request.Total,
                Moneda = string.IsNullOrWhiteSpace(request.Moneda) ? "USD" : request.Moneda.Trim(),
                RevCodigoSnap = request.RevCodigoSnap ?? string.Empty,
                UsuarioEmision = request.UsuarioEmision ?? string.Empty,
                IpEmision = request.IpEmision ?? string.Empty,
            };

            var f = await _repo.EmitirAsync(dto, context.CancellationToken);
            return FacturaGrpcMapper.ToReply(f);
        }
        catch (Exception ex) when (ex is not RpcException)
        {
            _logger.LogError(ex, "EmitirFactura");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<FacturaReply> ObtenerFacturaPorGuid(ObtenerFacturaRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.FacGuid, out var facGuid))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "fac_guid inválido."));

        var f = await _repo.ObtenerPorGuidAsync(facGuid, context.CancellationToken);
        if (f is null)
            throw new RpcException(new Status(StatusCode.NotFound, "Factura no encontrada."));

        return FacturaGrpcMapper.ToReply(f);
    }

    public override async Task<ListarMisFacturasReply> ListarMisFacturas(ListarMisFacturasRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.CliGuid, out var cliGuid))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "cli_guid inválido."));

        var page = request.Page <= 0 ? 1 : request.Page;
        var limit = request.Limit <= 0 ? 10 : request.Limit;

        var (items, total) = await _repo.ListarPorClienteAsync(cliGuid, page, limit, context.CancellationToken);
        var reply = new ListarMisFacturasReply { TotalFiltrado = total };
        foreach (var it in items)
            reply.Items.Add(FacturaGrpcMapper.ToReply(it));
        return reply;
    }
}
