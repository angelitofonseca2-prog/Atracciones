using Atracciones.Contracts.Identidad.V1;
using Atracciones.MsIdentidad.Business.Interfaces;
using Atracciones.MsIdentidad.DataManagement.Interfaces;
using Grpc.Core;

namespace Atracciones.MsIdentidad.Api.Grpc;

public sealed class UsuarioGrpcService : UsuarioService.UsuarioServiceBase
{
    private readonly IUsuarioProvisioningService _provisioning;
    private readonly IIdentidadUsuarioRepository _usuarios;
    private readonly ILogger<UsuarioGrpcService> _logger;

    public UsuarioGrpcService(
        IUsuarioProvisioningService provisioning,
        IIdentidadUsuarioRepository usuarios,
        ILogger<UsuarioGrpcService> logger)
    {
        _provisioning = provisioning;
        _usuarios = usuarios;
        _logger = logger;
    }

    public override async Task<UsuarioReply> CrearUsuario(
        CrearUsuarioRequest request,
        ServerCallContext context)
    {
        var roles = request.Roles.Count > 0
            ? request.Roles.ToList()
            : new List<string> { "CLIENTE" };

        try
        {
            var (usuId, usuGuid) = await _provisioning.CrearUsuarioAsync(
                request.Login,
                request.PasswordPlain,
                roles,
                string.IsNullOrWhiteSpace(request.CreadoPor) ? "grpc" : request.CreadoPor,
                string.IsNullOrWhiteSpace(request.IpCreador) ? "0.0.0.0" : request.IpCreador,
                context.CancellationToken);

            return new UsuarioReply
            {
                UsuId = usuId,
                UsuGuid = usuGuid.ToString("D"),
                Login = request.Login.Trim(),
            };
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "gRPC CrearUsuario conflicto");
            throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
        }
    }

    public override async Task<EliminarUsuarioReply> EliminarUsuario(
        EliminarUsuarioRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UsuGuid, out var guid))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "usu_guid inválido"));

        var ok = await _provisioning.EliminarUsuarioAsync(guid, context.CancellationToken);
        return new EliminarUsuarioReply { Ok = ok };
    }

    public override async Task<UsuarioReply> ObtenerUsuarioPorGuid(
        ObtenerUsuarioRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UsuGuid, out var guid))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "usu_guid inválido"));

        var u = await _usuarios.ObtenerActivoPorGuidAsync(guid, context.CancellationToken);
        if (u is null)
            throw new RpcException(new Status(StatusCode.NotFound, "Usuario no encontrado"));

        return new UsuarioReply
        {
            UsuId = u.UsuId,
            UsuGuid = u.UsuGuid.ToString("D"),
            Login = u.Login,
        };
    }
}
