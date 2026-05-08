using Microservicio.Atracciones.Business.DTOs.Admin.Resenias;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microservicio.Atracciones.Business.Mappers.Admin;
using Microservicio.Atracciones.Business.Validators.Admin;
using Microservicio.Atracciones.DataManagement.Interfaces;

namespace Microservicio.Atracciones.Business.Services.Admin
{
    public class ReseniaAdminService : IReseniaAdminService
    {
        private readonly IReseniaDataService _reseniaService;
        private readonly IAtraccionDataService _atraccionService;

        public ReseniaAdminService(IReseniaDataService reseniaService, IAtraccionDataService atraccionService)
        {
            _reseniaService = reseniaService;
            _atraccionService = atraccionService;
        }

        public async Task<ReseniaAdminResponse> ObtenerPorGuidAsync(Guid rsnGuid)
        {
            var model = await _reseniaService.ObtenerPorGuidAsync(rsnGuid)
                ?? throw new NotFoundException("Reseña", rsnGuid);

            var atraccion = await _atraccionService.ObtenerPorIdAsync(model.AtId);
            return ReseniaAdminMapper.ToResponse(model, atraccion?.AtNombre ?? string.Empty, string.Empty);
        }

        public async Task<IReadOnlyList<ReseniaAdminResponse>> ListarPorAtraccionAsync(Guid atGuid)
        {
            var atraccion = await _atraccionService.ObtenerPorGuidAsync(atGuid)
                ?? throw new NotFoundException("Atraccion", atGuid);

            var resenias = await _reseniaService.ListarPorAtraccionAsync(atraccion.AtId);
            return resenias.Select(r => ReseniaAdminMapper.ToResponse(r, atraccion.AtNombre, string.Empty)).ToList();
        }

        public async Task<ReseniaAdminResponse> ActualizarAsync(Guid rsnGuid, ActualizarReseniaRequest request, string usuarioAccion, string ip)
        {
            ReseniaAdminValidator.ValidarActualizar(request);

            var model = await _reseniaService.ObtenerPorGuidAsync(rsnGuid)
                ?? throw new NotFoundException("Reseña", rsnGuid);

            if (request.Rating is not null) model.RsnRating = request.Rating.Value;
            if (request.Comentario is not null) model.RsnComentario = request.Comentario;
            if (request.Estado is not null) model.RsnEstado = request.Estado.Value;
            model.RsnUsuarioMod = usuarioAccion;
            model.RsnIpMod = ip;

            await _reseniaService.ActualizarAsync(model);

            var atraccion = await _atraccionService.ObtenerPorIdAsync(model.AtId);
            return ReseniaAdminMapper.ToResponse(model, atraccion?.AtNombre ?? string.Empty, string.Empty);
        }

        public async Task EliminarAsync(Guid rsnGuid, string usuarioAccion, string ip)
        {
            var model = await _reseniaService.ObtenerPorGuidAsync(rsnGuid)
                ?? throw new NotFoundException("Reseña", rsnGuid);
            await _reseniaService.EliminarLogicoAsync(model.RsnId, usuarioAccion, ip);
        }
    }
}
