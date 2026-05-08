using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Mappers.Reservas;
using Microservicio.Atracciones.DataManagement.Models.Reservas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Services
{
    public class ReseniaDataService : IReseniaDataService
    {
        private readonly IUnitOfWork _uow;

        public ReseniaDataService(IUnitOfWork uow) => _uow = uow;

        public async Task<ReseniaDataModel?> ObtenerPorIdAsync(int rsnId)
            => ReseniaDataMapper.ToDataModel(await _uow.Resenias.ObtenerPorIdAsync(rsnId));

        public async Task<ReseniaDataModel?> ObtenerPorGuidAsync(Guid rsnGuid)
            => ReseniaDataMapper.ToDataModel(await _uow.Resenias.ObtenerPorGuidAsync(rsnGuid));

        public async Task<ReseniaDataModel?> ObtenerPorReservaAsync(int revId)
            => ReseniaDataMapper.ToDataModel(await _uow.Resenias.ObtenerPorReservaAsync(revId));

        public async Task<IReadOnlyList<ReseniaDataModel>> ListarPorAtraccionAsync(int atId)
        {
            var entities = await _uow.Resenias.ListarPorAtraccionAsync(atId);
            return entities.Select(e => ReseniaDataMapper.ToDataModel(e)!).ToList();
        }

        public async Task CrearAsync(ReseniaDataModel model)
        {
            var entity = ReseniaDataMapper.ToNewEntity(model);
            await _uow.Resenias.AgregarAsync(entity);
            await _uow.SaveChangesAsync();

            model.RsnId = entity.RsnId;
            model.RsnGuid = entity.RsnGuid;
        }

        public async Task ActualizarAsync(ReseniaDataModel model)
        {
            var entity = await _uow.Resenias.ObtenerPorIdAsync(model.RsnId)
                ?? throw new InvalidOperationException($"Reseña {model.RsnId} no encontrada.");

            ReseniaDataMapper.ApplyToEntity(model, entity);
            _uow.Resenias.Actualizar(entity);
            await _uow.SaveChangesAsync();
        }

        public async Task EliminarLogicoAsync(int rsnId, string usuarioAccion, string ip)
        {
            var entity = await _uow.Resenias.ObtenerPorIdAsync(rsnId)
                ?? throw new InvalidOperationException($"Reseña {rsnId} no encontrada.");

            entity.RsnEstado = 'I';
            entity.RsnFechaEliminacion = DateTime.UtcNow;
            entity.RsnUsuarioEliminacion = usuarioAccion;
            entity.RsnIpEliminacion = ip;

            _uow.Resenias.Actualizar(entity);
            await _uow.SaveChangesAsync();
        }

        public async Task<bool> YaTieneReseniaAsync(int revId)
            => await _uow.Resenias.ObtenerPorReservaAsync(revId) is not null;

        public Task<bool> YaTieneReseniaParaAtraccionAsync(int cliId, int atId)
            => _uow.Resenias.ExistePorClienteYAtraccionAsync(cliId, atId);
    }
}
