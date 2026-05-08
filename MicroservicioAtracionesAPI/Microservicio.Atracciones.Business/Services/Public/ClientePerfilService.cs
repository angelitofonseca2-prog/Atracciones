using Microservicio.Atracciones.Business.DTOs.Public.Clientes;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Public;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Clientes;

namespace Microservicio.Atracciones.Business.Services.Public
{
    public class ClientePerfilService : IClientePerfilService
    {
        private readonly IUsuarioDataService _usuarioService;
        private readonly IClienteDataService _clienteService;

        public ClientePerfilService(IUsuarioDataService usuarioService, IClienteDataService clienteService)
        {
            _usuarioService = usuarioService;
            _clienteService = clienteService;
        }

        public async Task<PerfilClienteResponse> ObtenerAsync(Guid usuGuid)
        {
            var cliente = await ObtenerClientePorUsuarioAsync(usuGuid);
            return ToResponse(cliente);
        }

        public async Task<PerfilClienteResponse> ActualizarAsync(Guid usuGuid, ActualizarPerfilClienteRequest request)
        {
            var cliente = await ObtenerClientePorUsuarioAsync(usuGuid);

            if (request.Nombres is not null) cliente.CliNombres = request.Nombres;
            if (request.Apellidos is not null) cliente.CliApellidos = request.Apellidos;
            if (request.Correo is not null) cliente.CliCorreo = request.Correo;
            if (request.Telefono is not null) cliente.CliTelefono = request.Telefono;

            await _clienteService.ActualizarAsync(cliente);
            return ToResponse(cliente);
        }

        private async Task<ClienteDataModel> ObtenerClientePorUsuarioAsync(Guid usuGuid)
        {
            var usuario = await _usuarioService.ObtenerPorGuidAsync(usuGuid)
                ?? throw new UnauthorizedBusinessException("El token no corresponde a un usuario activo.");

            return await _clienteService.ObtenerPorUsuarioIdAsync(usuario.UsuId)
                ?? throw new NotFoundException("Cliente asociado al usuario", usuGuid);
        }

        private static PerfilClienteResponse ToResponse(ClienteDataModel cliente)
            => new()
            {
                CliGuid = cliente.CliGuid,
                Nombres = cliente.CliNombres,
                Apellidos = cliente.CliApellidos,
                Correo = cliente.CliCorreo,
                Telefono = cliente.CliTelefono,
                TipoIdentificacion = cliente.CliTipoIdentificacion,
                NumeroIdentificacion = cliente.CliNumeroIdentificacion
            };
    }
}
