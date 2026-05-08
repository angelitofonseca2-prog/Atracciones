using Microservicio.Atracciones.DataAccess.Queries;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Mappers.Facturacion;
using Microservicio.Atracciones.DataManagement.Models.Common;
using Microservicio.Atracciones.DataManagement.Models.Facturacion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Services
{
    public class FacturaDataService : IFacturaDataService
    {
        private readonly IUnitOfWork _uow;
        private readonly FacturaQueryRepository _queryRepo;

        public FacturaDataService(IUnitOfWork uow, FacturaQueryRepository queryRepo)
        {
            _uow = uow;
            _queryRepo = queryRepo;
        }

        public async Task<FacturaDataModel?> ObtenerPorIdAsync(int facId)
            => FacturaDataMapper.ToDataModel(await _uow.Facturas.ObtenerPorIdAsync(facId));

        public async Task<FacturaDataModel?> ObtenerPorGuidAsync(Guid facGuid)
            => FacturaDataMapper.ToDataModel(
                await _queryRepo.ObtenerFacturaCompletaAsync(facGuid));

        public async Task<FacturaDataModel?> ObtenerPorReservaAsync(int revId)
            => FacturaDataMapper.ToDataModel(await _uow.Facturas.ObtenerPorReservaAsync(revId));

        public async Task<DataPagedResult<FacturaDataModel>> ListarAdminAsync(int page, int limit, char? estado = null)
        {
            var paged = await _queryRepo.ListarAdminAsync(page, limit, estado);
            var items = paged.Items.Select(e => FacturaDataMapper.ToDataModel(e)!).ToList();
            return new DataPagedResult<FacturaDataModel>(items, paged.TotalFiltrado, paged.TotalSinFiltros, paged.Page, paged.Limit);
        }

        public async Task<DataPagedResult<FacturaDataModel>> ListarPorClienteAsync(int cliId, int page, int limit)
        {
            var paged = await _queryRepo.ListarPorClienteAsync(cliId, page, limit);
            var items = paged.Items.Select(e => FacturaDataMapper.ToDataModel(e)!).ToList();
            return new DataPagedResult<FacturaDataModel>(items, paged.TotalFiltrado, paged.TotalSinFiltros, paged.Page, paged.Limit);
        }

        public async Task CrearAsync(FacturaDataModel model)
        {
            var facEntity = FacturaDataMapper.ToNewEntity(model);
            await _uow.Facturas.AgregarAsync(facEntity);
            await _uow.SaveChangesAsync();

            model.FacId = facEntity.FacId;
            model.FacGuid = facEntity.FacGuid;
            model.FacFechaEmision = facEntity.FacFechaEmision;
            model.FacEstado = facEntity.FacEstado;

            // Crea datos de facturación en la misma transacción
            if (model.DatosFacturacion is not null)
            {
                model.DatosFacturacion.FacId = facEntity.FacId;
                var datosEntity = FacturaDataMapper.ToNewDatosEntity(model.DatosFacturacion);
                await _uow.Facturas.AgregarDatosFacturacionAsync(datosEntity);
                await _uow.SaveChangesAsync();

                model.DatosFacturacion.DfacId = datosEntity.DfacId;
            }
        }

        public async Task InhabilitarAsync(int facId, string motivo, string usuarioAccion, string ip)
        {
            var entity = await _uow.Facturas.ObtenerPorIdAsync(facId)
                ?? throw new InvalidOperationException($"Factura {facId} no encontrada.");

            entity.FacEstado = 'I';
            entity.FacMotivoInhabilitacion = motivo;
            entity.FacFechaEliminacion = DateTime.UtcNow;
            entity.FacUsuarioEliminacion = usuarioAccion;
            entity.FacIpEliminacion = ip;

            _uow.Facturas.Actualizar(entity);
            await _uow.SaveChangesAsync();
        }

        public async Task<bool> ExisteNumeroFacturaAsync(string numero)
            => await _queryRepo.ObtenerPorNumeroAsync(numero) is not null;

        public async Task<string> GenerarNumeroSecuencialAsync(int anio)
        {
            var maxSecuencial = await _queryRepo.ObtenerMaxSecuencialPorAnioAsync(anio);
            return $"FAC-{anio}-{maxSecuencial + 1:000000}";
        }
    }
}
