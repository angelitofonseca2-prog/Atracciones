using Microservicio.Atracciones.Business.DTOs.Admin.Clientes;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microservicio.Atracciones.Business.Interfaces.Auth;
using Microservicio.Atracciones.Business.Interfaces.Integration;
using Microservicio.Atracciones.Business.Mappers.Admin;
using Microservicio.Atracciones.Business.Validators.Admin;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Clientes;
using Microservicio.Atracciones.DataManagement.Models.Common;
using Microservicio.Atracciones.DataManagement.Models.Seguridad;

namespace Microservicio.Atracciones.Business.Services.Admin
{
    public class ClienteAdminService : IClienteAdminService
    {
        private readonly IClienteDataService _clienteService;
        private readonly IUsuarioDataService _usuarioService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentidadUsuarioSyncPublisher _identidadSync;
        private readonly IClienteCrmSyncPublisher _crmSync;

        public ClienteAdminService(
            IClienteDataService clienteService,
            IUsuarioDataService usuarioService,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork,
            IIdentidadUsuarioSyncPublisher identidadSync,
            IClienteCrmSyncPublisher crmSync)
        {
            _clienteService = clienteService;
            _usuarioService = usuarioService;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _identidadSync = identidadSync;
            _crmSync = crmSync;
        }

        public async Task<ClienteResponse> CrearAsync(CrearClienteRequest request, string usuarioAccion, string ip)
        {
            ClienteAdminValidator.ValidarCrear(request);

            if (await _usuarioService.ExisteLoginAsync(request.Login))
                throw new ConflictException($"El login '{request.Login}' ya está en uso.");

            var clienteExistente = await _clienteService.ObtenerPorNumeroIdentificacionAsync(request.NumeroIdentificacion);
            if (clienteExistente is not null && clienteExistente.UsuId.HasValue)
                throw new ConflictException($"Ya existe un cliente con la identificación '{request.NumeroIdentificacion}' vinculado a una cuenta.");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Crear usuario de acceso
                var usuarioModel = new UsuarioDataModel
                {
                    UsuLogin = request.Login,
                    UsuPasswordHash = _passwordHasher.Hashear(request.Password),
                    UsuEstado = 'A',
                    UsuUsuarioRegistro = usuarioAccion,
                    UsuIpRegistro = ip,
                    Roles = new List<RolDataModel>
                    {
                        new() { RolDescripcion = "CLIENTE" }
                    }
                };
                try
                {
                    await _usuarioService.CrearAsync(usuarioModel);
                }
                catch (InvalidOperationException ex) when (ex.Message.StartsWith("Roles no encontrados"))
                {
                    throw new ConflictException(ex.Message);
                }

                if (clienteExistente is not null)
                {
                    clienteExistente.UsuId = usuarioModel.UsuId;
                    clienteExistente.CliTipoIdentificacion = request.TipoIdentificacion;
                    clienteExistente.CliNombres = request.Nombres;
                    clienteExistente.CliApellidos = request.Apellidos;
                    clienteExistente.CliRazonSocial = request.RazonSocial;
                    clienteExistente.CliCorreo = request.Correo;
                    clienteExistente.CliTelefono = request.Telefono;
                    clienteExistente.CliDireccion = request.Direccion;

                    await _clienteService.ActualizarAsync(clienteExistente);
                    await transaction.CommitAsync();
                    await MirrorIdentidadSiAdminAsync(usuarioAccion, usuarioModel, clienteExistente.CliId);
                    await MirrorCrmAsync(usuarioModel, clienteExistente, usuarioAccion, ip);
                    return ClienteAdminMapper.ToResponse(clienteExistente);
                }

                // 2. Crear cliente vinculado al usuario
                var clienteModel = new ClienteDataModel
                {
                    UsuId = usuarioModel.UsuId,
                    CliTipoIdentificacion = request.TipoIdentificacion,
                    CliNumeroIdentificacion = request.NumeroIdentificacion,
                    CliNombres = request.Nombres,
                    CliApellidos = request.Apellidos,
                    CliRazonSocial = request.RazonSocial,
                    CliCorreo = request.Correo,
                    CliTelefono = request.Telefono,
                    CliDireccion = request.Direccion,
                    CliEstado = 'A',
                    CliUsuarioIngreso = usuarioAccion,
                    CliIpIngreso = ip
                };
                await _clienteService.CrearAsync(clienteModel);

                await transaction.CommitAsync();
                await MirrorIdentidadSiAdminAsync(usuarioAccion, usuarioModel, clienteModel.CliId);
                await MirrorCrmAsync(usuarioModel, clienteModel, usuarioAccion, ip);
                return ClienteAdminMapper.ToResponse(clienteModel);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task MirrorIdentidadSiAdminAsync(
            string usuarioAccion,
            UsuarioDataModel usuarioModel,
            int? cliId)
        {
            if (string.Equals(usuarioAccion, "publico", StringComparison.OrdinalIgnoreCase))
                return;

            var roles = usuarioModel.Roles.Select(r => r.RolDescripcion.ToUpperInvariant()).ToList();
            await _identidadSync.SincronizarYObtenerTokenAsync(
                new IdentidadUsuarioEspejo(
                    usuarioModel.UsuId,
                    usuarioModel.UsuGuid,
                    usuarioModel.UsuLogin,
                    usuarioModel.UsuPasswordHash,
                    cliId,
                    roles));
        }

        private Task MirrorCrmAsync(
            UsuarioDataModel usuario,
            ClienteDataModel cliente,
            string usuarioAccion,
            string ip)
            => _crmSync.EspejarAsync(ToCrmEspejo(usuario, cliente, usuarioAccion, ip));

        private async Task MirrorCrmSiUsuarioVinculadoAsync(
            ClienteDataModel cliente,
            string usuarioAccion,
            string ip)
        {
            if (!cliente.UsuId.HasValue)
                return;
            var usuario = await _usuarioService.ObtenerPorIdAsync(cliente.UsuId.Value);
            if (usuario is null)
                return;
            await MirrorCrmAsync(usuario, cliente, usuarioAccion, ip);
        }

        private static ClienteCrmEspejo ToCrmEspejo(
            UsuarioDataModel usuario,
            ClienteDataModel cliente,
            string usuarioAccion,
            string ip)
            => new(
                usuario.UsuGuid,
                cliente.CliTipoIdentificacion,
                cliente.CliNumeroIdentificacion,
                cliente.CliNombres,
                cliente.CliApellidos,
                cliente.CliRazonSocial,
                cliente.CliCorreo,
                cliente.CliTelefono,
                cliente.CliDireccion,
                usuarioAccion,
                ip);

        public async Task<ClienteResponse> ActualizarAsync(Guid cliGuid, ActualizarClienteRequest request, string usuarioAccion, string ip)
        {
            var model = await _clienteService.ObtenerPorGuidAsync(cliGuid)
                ?? throw new NotFoundException("Cliente", cliGuid);

            if (request.Correo is not null) model.CliCorreo = request.Correo;
            if (request.Telefono is not null) model.CliTelefono = request.Telefono;
            if (request.Direccion is not null) model.CliDireccion = request.Direccion;
            if (request.Nombres is not null) model.CliNombres = request.Nombres;
            if (request.Apellidos is not null) model.CliApellidos = request.Apellidos;
            if (request.RazonSocial is not null) model.CliRazonSocial = request.RazonSocial;

            await _clienteService.ActualizarAsync(model);
            await MirrorCrmSiUsuarioVinculadoAsync(model, usuarioAccion, ip);
            return ClienteAdminMapper.ToResponse(model);
        }

        public async Task EliminarAsync(Guid cliGuid, string usuarioAccion, string ip)
        {
            var model = await _clienteService.ObtenerPorGuidAsync(cliGuid)
                ?? throw new NotFoundException("Cliente", cliGuid);
            await _clienteService.EliminarLogicoAsync(model.CliId, usuarioAccion, ip);
        }

        public async Task<ClienteResponse> ObtenerPorGuidAsync(Guid cliGuid)
        {
            var model = await _clienteService.ObtenerPorGuidAsync(cliGuid)
                ?? throw new NotFoundException("Cliente", cliGuid);
            return ClienteAdminMapper.ToResponse(model);
        }

        public async Task<DataPagedResult<ClienteResponse>> ListarAsync(ClienteFiltroRequest filtro)
        {
            var todos = await _clienteService.ListarAsync();
            var filtrados = todos
                .Where(c => filtro.Estado is null || c.CliEstado == filtro.Estado)
                .ToList();

            var items = filtrados
                .Skip((filtro.Page - 1) * filtro.Limit)
                .Take(filtro.Limit)
                .Select(c => ClienteAdminMapper.ToResponse(c))
                .ToList();

            return new DataPagedResult<ClienteResponse>(items, filtrados.Count, todos.Count, filtro.Page, filtro.Limit);
        }
    }
}
