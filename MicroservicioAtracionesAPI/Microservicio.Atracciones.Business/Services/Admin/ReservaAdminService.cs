using Microservicio.Atracciones.Business.DTOs.Admin.Reservas;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microservicio.Atracciones.Business.Mappers.Admin;
using Microservicio.Atracciones.Business.Rules.Admin;
using Microservicio.Atracciones.Business.Validators.Admin;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Common;
using Microservicio.Atracciones.DataManagement.Models.Reservas;

namespace Microservicio.Atracciones.Business.Services.Admin
{
    public class ReservaAdminService : IReservaAdminService
    {
        private readonly IReservaDataService _reservaService;
        private readonly ReservaAdminRules _rules;

        public ReservaAdminService(IReservaDataService reservaService, ReservaAdminRules rules)
        {
            _reservaService = reservaService;
            _rules = rules;
        }

        public async Task<ReservaAdminResponse> ObtenerPorGuidAsync(Guid revGuid)
        {
            var model = await _reservaService.ObtenerPorGuidAsync(revGuid)
                ?? throw new NotFoundException("Reserva", revGuid);
            return ReservaAdminMapper.ToResponse(model, string.Empty, string.Empty);
        }

        public async Task<DataPagedResult<ReservaAdminResponse>> ListarAsync(ReservaAdminFiltroRequest filtro)
        {
            var dmFiltro = new ReservaFiltroDataModel
            {
                Estado = filtro.Estado,
                FechaDesde = filtro.FechaDesde,
                FechaHasta = filtro.FechaHasta,
                Page = filtro.Page,
                Limit = filtro.Limit
            };

            var paged = await _reservaService.ListarAdminAsync(dmFiltro);
            var items = paged.Items.Select(m => ReservaAdminMapper.ToResponse(m, string.Empty, string.Empty)).ToList();
            return new DataPagedResult<ReservaAdminResponse>(items, paged.TotalFiltrado, paged.TotalSinFiltros, paged.Page, paged.Limit);
        }

        public async Task ActualizarEstadoAsync(Guid revGuid, ActualizarEstadoReservaRequest request, string usuarioAccion, string ip)
        {
            ReservaAdminValidator.ValidarActualizarEstado(request);
            await _rules.ValidarCambioEstadoAsync(revGuid, request.NuevoEstado);

            var model = await _reservaService.ObtenerPorGuidAsync(revGuid)!;
            await _reservaService.ActualizarEstadoAsync(model!.RevId, request.NuevoEstado, request.Motivo, usuarioAccion, ip);
        }
    }

}
