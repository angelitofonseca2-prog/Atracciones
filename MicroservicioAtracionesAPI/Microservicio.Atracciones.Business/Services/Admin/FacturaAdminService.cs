using Microservicio.Atracciones.Business.DTOs.Admin.Facturas;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microservicio.Atracciones.Business.Mappers.Admin;
using Microservicio.Atracciones.Business.Rules.Admin;
using Microservicio.Atracciones.Business.Validators.Admin;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Common;
using Microservicio.Atracciones.DataManagement.Models.Facturacion;

namespace Microservicio.Atracciones.Business.Services.Admin
{
    public class FacturaAdminService : IFacturaAdminService
    {
        private readonly IFacturaDataService _facturaService;
        private readonly IReservaDataService _reservaService;
        private readonly FacturaRules _rules;

        public FacturaAdminService(IFacturaDataService facturaService, IReservaDataService reservaService, FacturaRules rules)
        {
            _facturaService = facturaService;
            _reservaService = reservaService;
            _rules = rules;
        }

        public async Task<FacturaResponse> GenerarAsync(GenerarFacturaRequest request, string usuarioAccion, string ip)
        {
            FacturaAdminValidator.ValidarGenerar(request);
            await _rules.ValidarPuedeFacturarAsync(request.RevGuid);

            var reserva = await _reservaService.ObtenerPorGuidAsync(request.RevGuid)!;

            var model = new FacturaDataModel
            {
                RevId = reserva!.RevId,
                FacNumero = FacturaRules.GenerarNumeroFactura(),
                FacTotal = reserva.RevTotal,
                FacObservacion = request.Observacion,
                FacEstado = 'A',
                FacUsuarioIngreso = usuarioAccion,
                FacIpIngreso = ip,
                DatosFacturacion = new DatosFacturacionDataModel
                {
                    DfacNombre = request.NombreReceptor,
                    DfacApellido = request.ApellidoReceptor,
                    DfacCorreo = request.CorreoReceptor,
                    DfacTelefono = request.TelefonoReceptor
                }
            };

            await _facturaService.CrearAsync(model);
            return FacturaAdminMapper.ToResponse(model, reserva.RevCodigo);
        }

        public async Task<FacturaResponse> ObtenerPorGuidAsync(Guid facGuid)
        {
            var model = await _facturaService.ObtenerPorGuidAsync(facGuid)
                ?? throw new NotFoundException("Factura", facGuid);

            var reserva = await _reservaService.ObtenerPorIdAsync(model.RevId);
            return FacturaAdminMapper.ToResponse(model, reserva?.RevCodigo ?? string.Empty);
        }

        public async Task<DataPagedResult<FacturaResponse>> ListarAsync(int page, int limit)
        {
            var paged = await _facturaService.ListarAdminAsync(page, limit);
            var items = new List<FacturaResponse>();

            foreach (var f in paged.Items)
            {
                var reserva = await _reservaService.ObtenerPorIdAsync(f.RevId);
                items.Add(FacturaAdminMapper.ToResponse(f, reserva?.RevCodigo ?? string.Empty));
            }

            return new DataPagedResult<FacturaResponse>(items, paged.TotalFiltrado, paged.TotalSinFiltros, paged.Page, paged.Limit);
        }
    }
}
