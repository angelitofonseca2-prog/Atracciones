using Microservicio.Atracciones.Business.DTOs.Admin.Usuarios;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microservicio.Atracciones.Business.Interfaces.Auth;
using Microservicio.Atracciones.Business.Mappers.Admin;
using Microservicio.Atracciones.Business.Validators.Admin;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Seguridad;

namespace Microservicio.Atracciones.Business.Services.Admin
{
    public class UsuarioAdminService : IUsuarioAdminService
    {
        private readonly IUsuarioDataService _usuarioService;
        private readonly IPasswordHasher _passwordHasher;

        public UsuarioAdminService(IUsuarioDataService usuarioService, IPasswordHasher passwordHasher)
        {
            _usuarioService = usuarioService;
            _passwordHasher = passwordHasher;
        }

        public async Task<UsuarioResponse> CrearAsync(CrearUsuarioRequest request, string usuarioAccion, string ip)
        {
            UsuarioAdminValidator.ValidarCrear(request);

            if (await _usuarioService.ExisteLoginAsync(request.Login))
                throw new ConflictException($"El login '{request.Login}' ya está en uso.");

            var model = new UsuarioDataModel
            {
                UsuLogin = request.Login,
                UsuPasswordHash = _passwordHasher.Hashear(request.Password),
                UsuEstado = 'A',
                UsuUsuarioRegistro = usuarioAccion,
                UsuIpRegistro = ip,
                Roles = request.Roles
                    .Select(r => new RolDataModel { RolDescripcion = r.Trim().ToUpperInvariant() })
                    .ToList()
            };

            try
            {
                await _usuarioService.CrearAsync(model);
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("Roles no encontrados"))
            {
                throw new ConflictException(ex.Message);
            }

            return UsuarioAdminMapper.ToResponse(model);
        }

        public async Task<UsuarioResponse> ActualizarAsync(Guid usuGuid, ActualizarUsuarioRequest request, string usuarioAccion, string ip)
        {
            var model = await _usuarioService.ObtenerPorGuidAsync(usuGuid)
                ?? throw new NotFoundException("Usuario", usuGuid);

            if (!string.IsNullOrWhiteSpace(request.NuevaPassword))
            {
                model.UsuPasswordHash = _passwordHasher.Hashear(request.NuevaPassword);
                model.UsuUsuarioMod = usuarioAccion;
                model.UsuIpMod = ip;
                model.UsuFechaMod = DateTime.UtcNow;
                await _usuarioService.ActualizarAsync(model);
            }

            return UsuarioAdminMapper.ToResponse(model);
        }

        public async Task EliminarAsync(Guid usuGuid, string usuarioAccion, string ip)
        {
            var model = await _usuarioService.ObtenerPorGuidAsync(usuGuid)
                ?? throw new NotFoundException("Usuario", usuGuid);

            await _usuarioService.EliminarLogicoAsync(model.UsuId, usuarioAccion, ip);
        }

        public async Task<IReadOnlyList<UsuarioResponse>> ListarAsync()
        {
            var modelList = await _usuarioService.ListarAsync();
            return modelList.Select(UsuarioAdminMapper.ToResponse).ToList();
        }
    }

}
