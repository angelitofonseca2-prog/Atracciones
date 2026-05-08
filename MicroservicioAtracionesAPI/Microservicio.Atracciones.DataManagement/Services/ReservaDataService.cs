using Microservicio.Atracciones.DataAccess.Queries;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Mappers.Reservas;
using Microservicio.Atracciones.DataManagement.Models.Common;
using Microservicio.Atracciones.DataManagement.Models.Reservas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Services
{
    public class ReservaDataService : IReservaDataService
    {
        private readonly IUnitOfWork _uow;
        private readonly ReservaQueryRepository _queryRepo;

        public ReservaDataService(IUnitOfWork uow, ReservaQueryRepository queryRepo)
        {
            _uow = uow;
            _queryRepo = queryRepo;
        }

        public async Task<ReservaDataModel?> ObtenerPorIdAsync(int revId)
            => ReservaDataMapper.ToDataModel(await _uow.Reservas.ObtenerPorIdAsync(revId));

        public async Task<ReservaDataModel?> ObtenerPorGuidAsync(Guid revGuid)
            => ReservaDataMapper.ToDataModel(
                await _queryRepo.ObtenerDetalleCompletoAsync(revGuid));

        public async Task<DataPagedResult<ReservaDataModel>> ListarPorClienteAsync(int cliId, int page, int limit)
        {
            var paged = await _queryRepo.ListarPorClienteAsync(cliId, page, limit);
            var items = paged.Items.Select(e => ReservaDataMapper.ToDataModel(e)!).ToList();
            return new DataPagedResult<ReservaDataModel>(items, paged.TotalFiltrado, paged.TotalSinFiltros, paged.Page, paged.Limit);
        }

        public async Task<DataPagedResult<ReservaDataModel>> ListarAdminAsync(ReservaFiltroDataModel filtro)
        {
            var filtroQuery = new ReservaFiltroQuery(
                CliId: filtro.CliId,
                AtId: filtro.AtId,
                Estado: filtro.Estado,
                FechaDesde: filtro.FechaDesde,
                FechaHasta: filtro.FechaHasta,
                Page: filtro.Page,
                Limit: filtro.Limit
            );

            var paged = await _queryRepo.ListarAdminAsync(filtroQuery);
            var items = paged.Items.Select(e => ReservaDataMapper.ToDataModel(e)!).ToList();
            return new DataPagedResult<ReservaDataModel>(items, paged.TotalFiltrado, paged.TotalSinFiltros, paged.Page, paged.Limit);
        }

        public async Task CrearAsync(ReservaDataModel model)
        {
            var entity = ReservaDataMapper.ToNewEntity(model);

            // Añade cabecera Y detalle al contexto antes de cualquier SaveChanges.
            // EF Core resuelve las FK en memoria → un único commit atómico.
            await _uow.Reservas.AgregarAsync(entity);

            foreach (var linea in model.Detalle)
            {
                var detalleEntity = ReservaDataMapper.ToNewDetalleEntity(linea);
                detalleEntity.RevId = 0;          // EF lo resuelve por la FK del navigation
                entity.ReservasDetalle.Add(detalleEntity);
            }

            await _uow.SaveChangesAsync();   // #3: único commit — operación atómica

            // Sincronizar IDs generados de vuelta al model
            model.RevId = entity.RevId;
            model.RevGuid = entity.RevGuid;
        }

        public async Task ActualizarEstadoAsync(int revId, char nuevoEstado, string motivo, string usuarioAccion, string ip)
        {
            var entity = await _uow.Reservas.ObtenerPorIdAsync(revId)
                ?? throw new InvalidOperationException($"Reserva {revId} no encontrada.");

            entity.RevEstado = nuevoEstado;

            if (nuevoEstado == 'C')
            {
                entity.RevFechaCancelacion = DateTime.UtcNow;
                entity.RevUsuarioCancelacion = usuarioAccion;
                entity.RevIpCancelacion = ip;
                entity.RevMotivoCancelacion = motivo;
            }
            else
            {
                entity.RevFechaMod = DateTime.UtcNow;
                entity.RevUsuarioMod = usuarioAccion;
                entity.RevIpMod = ip;
            }

            _uow.Reservas.Actualizar(entity);
            await _uow.SaveChangesAsync();
        }

        public async Task<bool> ExisteCodigoAsync(string codigo)
            => await _uow.Reservas.ObtenerPorCodigoAsync(codigo) is not null;
    }

}
