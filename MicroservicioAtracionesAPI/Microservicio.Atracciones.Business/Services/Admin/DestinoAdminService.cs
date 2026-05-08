using Microservicio.Atracciones.Business.DTOs.Admin.Destinos;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microservicio.Atracciones.Business.Mappers.Admin;
using Microservicio.Atracciones.Business.Validators.Admin;
using Microservicio.Atracciones.DataManagement.Interfaces;

namespace Microservicio.Atracciones.Business.Services.Admin
{
    public class DestinoAdminService : IDestinoAdminService
    {
        private readonly IDestinoDataService _destinoService;

        public DestinoAdminService(IDestinoDataService destinoService)
            => _destinoService = destinoService;

        public async Task<DestinoResponse> CrearAsync(CrearDestinoRequest request, string usuarioAccion, string ip)
        {
            DestinoAdminValidator.ValidarCrear(request);

            var model = new DataManagement.Models.Catalogos.DestinoDataModel
            {
                DesNombre = request.Nombre,
                DesPais = request.Pais,
                DesImagenUrl = request.ImagenUrl,
                DesEstado = 'A',
                DesUsuarioIngreso = usuarioAccion,
                DesIpIngreso = ip
            };

            await _destinoService.CrearAsync(model);
            return DestinoAdminMapper.ToResponse(model);
        }

        public async Task<DestinoResponse> ActualizarAsync(Guid desGuid, ActualizarDestinoRequest request, string usuarioAccion, string ip)
        {
            var model = await _destinoService.ObtenerPorGuidAsync(desGuid)
                ?? throw new NotFoundException("Destino", desGuid);

            if (request.Nombre is not null) model.DesNombre = request.Nombre;
            if (request.Pais is not null) model.DesPais = request.Pais;
            if (request.ImagenUrl is not null) model.DesImagenUrl = request.ImagenUrl;
            model.DesUsuarioMod = usuarioAccion;
            model.DesIpMod = ip;

            await _destinoService.ActualizarAsync(model);
            return DestinoAdminMapper.ToResponse(model);
        }

        public async Task EliminarAsync(Guid desGuid, string usuarioAccion, string ip)
        {
            var model = await _destinoService.ObtenerPorGuidAsync(desGuid)
                ?? throw new NotFoundException("Destino", desGuid);
            await _destinoService.EliminarLogicoAsync(model.DesId, usuarioAccion, ip);
        }

        public async Task<IReadOnlyList<DestinoResponse>> ListarAsync()
        {
            var todos = await _destinoService.ListarActivosAsync();
            return todos.Select(d => DestinoAdminMapper.ToResponse(d)).ToList();
        }
    }
}
