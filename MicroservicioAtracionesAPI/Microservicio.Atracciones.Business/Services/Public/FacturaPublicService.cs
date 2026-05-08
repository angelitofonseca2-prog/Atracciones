using Microservicio.Atracciones.Business.DTOs.Admin.Facturas;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Public;
using Microservicio.Atracciones.Business.Mappers.Admin;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.Business.Services.Public
{
    public class FacturaPublicService : IFacturaPublicService
    {
        private readonly IFacturaDataService _facturaService;
        private readonly IReservaDataService _reservaService;
        private readonly IUsuarioDataService _usuarioService;
        private readonly IClienteDataService _clienteService;

        public FacturaPublicService(
            IFacturaDataService facturaService,
            IReservaDataService reservaService,
            IUsuarioDataService usuarioService,
            IClienteDataService clienteService)
        {
            _facturaService = facturaService;
            _reservaService = reservaService;
            _usuarioService = usuarioService;
            _clienteService = clienteService;
        }

        public async Task<DataPagedResult<FacturaResponse>> ListarMisFacturasAsync(Guid usuGuid, int page, int limit)
        {
            var usuario = await _usuarioService.ObtenerPorGuidAsync(usuGuid)
                ?? throw new UnauthorizedBusinessException("El token no corresponde a un usuario activo.");

            var cliente = await _clienteService.ObtenerPorUsuarioIdAsync(usuario.UsuId)
                ?? throw new NotFoundException("Cliente asociado al usuario", usuGuid);

            var paged = await _facturaService.ListarPorClienteAsync(cliente.CliId, page, limit);
            var items = new List<FacturaResponse>();

            foreach (var factura in paged.Items)
            {
                var reserva = await _reservaService.ObtenerPorIdAsync(factura.RevId);
                items.Add(FacturaAdminMapper.ToResponse(factura, reserva?.RevCodigo ?? string.Empty));
            }

            return new DataPagedResult<FacturaResponse>(items, paged.TotalFiltrado, paged.TotalSinFiltros, paged.Page, paged.Limit);
        }
    }
}
