using Atracciones.Contracts.Clientes.V1;
using Atracciones.MsClientes.DataManagement.Interfaces;
using Atracciones.MsClientes.DataManagement.Models;
using Grpc.Core;

namespace Atracciones.MsClientes.Api.Grpc;

public sealed class ClienteGrpcService : ClienteService.ClienteServiceBase
{
    private readonly IClienteRepository _repo;
    private readonly ILogger<ClienteGrpcService> _logger;

    public ClienteGrpcService(IClienteRepository repo, ILogger<ClienteGrpcService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public override async Task<ClienteReply> CrearCliente(CrearClienteRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UsuGuid, out var guid))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "usu_guid inválido"));

        try
        {
            var dto = await _repo.CrearAsync(new CrearClienteInternoDto
            {
                CliGuid = guid,
                TipoIdentificacion = request.TipoIdentificacion,
                NumeroIdentificacion = request.NumeroIdentificacion,
                Nombres = NullIfEmpty(request.Nombres),
                Apellidos = NullIfEmpty(request.Apellidos),
                RazonSocial = NullIfEmpty(request.RazonSocial),
                Correo = request.Correo,
                Telefono = NullIfEmpty(request.Telefono),
                Direccion = NullIfEmpty(request.Direccion),
                CreadoPor = string.IsNullOrWhiteSpace(request.CreadoPor) ? "grpc" : request.CreadoPor,
                IpCreador = string.IsNullOrWhiteSpace(request.IpCreador) ? "0.0.0.0" : request.IpCreador,
            }, context.CancellationToken);
            return Map(dto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "CrearCliente conflicto");
            throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
        }
    }

    public override async Task<ClienteReply> ObtenerClientePorGuid(ObtenerClienteRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.CliGuid, out var guid))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "cli_guid inválido"));

        var dto = await _repo.ObtenerActivoPorGuidAsync(guid, context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Cliente no encontrado"));
        return Map(dto);
    }

    public override async Task<ClienteReply> ObtenerClientePorNumeroIdentificacion(
        ObtenerClientePorDocRequest request,
        ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.NumeroIdentificacion))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "numero_identificacion obligatorio"));

        var dto = await _repo.ObtenerActivoPorNumeroIdentificacionAsync(request.NumeroIdentificacion.Trim(), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Cliente no encontrado"));
        return Map(dto);
    }

    public override async Task<ClienteReply> ActualizarCliente(ActualizarClienteRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.CliGuid, out var guid))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "cli_guid inválido"));

        var dto = await _repo.ActualizarCamposAsync(guid, new ActualizarClienteInternoDto
        {
            TipoIdentificacion = NullIfEmpty(request.TipoIdentificacion),
            NumeroIdentificacion = NullIfEmpty(request.NumeroIdentificacion),
            Nombres = request.Nombres,
            Apellidos = request.Apellidos,
            RazonSocial = request.RazonSocial,
            Correo = NullIfEmpty(request.Correo),
            Telefono = request.Telefono,
            Direccion = request.Direccion,
        }, context.CancellationToken);

        if (dto is null)
            throw new RpcException(new Status(StatusCode.NotFound, "Cliente no encontrado"));
        return Map(dto);
    }

    public override async Task<EliminarClienteReply> EliminarCliente(EliminarClienteRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.CliGuid, out var guid))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "cli_guid inválido"));

        var ok = await _repo.MarcarInactivoAsync(guid, context.CancellationToken);
        return new EliminarClienteReply { Ok = ok };
    }

    private static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s;

    private static ClienteReply Map(ClienteDto d) => new()
    {
        CliGuid = d.CliGuid.ToString("D"),
        TipoIdentificacion = d.TipoIdentificacion,
        NumeroIdentificacion = d.NumeroIdentificacion,
        Nombres = d.Nombres ?? "",
        Apellidos = d.Apellidos ?? "",
        RazonSocial = d.RazonSocial ?? "",
        Correo = d.Correo,
        Telefono = d.Telefono ?? "",
        Direccion = d.Direccion ?? "",
    };
}
