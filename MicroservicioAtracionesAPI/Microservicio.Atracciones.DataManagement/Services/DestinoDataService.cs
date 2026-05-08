using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using Microservicio.Atracciones.DataAccess.Queries;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Mappers.Catalogos;
using Microservicio.Atracciones.DataManagement.Mappers.Atracciones;
using Microservicio.Atracciones.DataManagement.Models.Catalogos;
using Microservicio.Atracciones.DataManagement.Models.Atracciones;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.DataManagement.Services
{
    public class DestinoDataService : IDestinoDataService
    {
        private readonly IUnitOfWork _uow;

        public DestinoDataService(IUnitOfWork uow) => _uow = uow;

        public async Task<DestinoDataModel?> ObtenerPorIdAsync(int desId)
        {
            var entity = await _uow.Destinos.ObtenerPorIdAsync(desId);
            return DestinoDataMapper.ToDataModel(entity);
        }

        public async Task<DestinoDataModel?> ObtenerPorGuidAsync(Guid desGuid)
        {
            var entity = await _uow.Destinos.ObtenerPorGuidAsync(desGuid);
            return DestinoDataMapper.ToDataModel(entity);
        }

        public async Task<DestinoDataModel?> ObtenerPorNombreAsync(string nombre)
        {
            var entity = await _uow.Destinos.ObtenerPorNombreAsync(nombre);
            return DestinoDataMapper.ToDataModel(entity);
        }

        public async Task<IReadOnlyList<DestinoDataModel>> ListarActivosAsync()
        {
            var entities = await _uow.Destinos.ListarActivosAsync();
            return entities.Select(e => DestinoDataMapper.ToDataModel(e)!).ToList();
        }

        public async Task CrearAsync(DestinoDataModel model)
        {
            var entity = DestinoDataMapper.ToNewEntity(model);
            await _uow.Destinos.AgregarAsync(entity);
            await _uow.SaveChangesAsync();

            model.DesId = entity.DesId;
            model.DesGuid = entity.DesGuid;
        }

        public async Task ActualizarAsync(DestinoDataModel model)
        {
            var entity = await _uow.Destinos.ObtenerPorIdAsync(model.DesId)
                ?? throw new InvalidOperationException($"Destino {model.DesId} no encontrado.");

            DestinoDataMapper.ApplyToEntity(model, entity);
            _uow.Destinos.Actualizar(entity);
            await _uow.SaveChangesAsync();
        }

        public async Task EliminarLogicoAsync(int desId, string usuarioAccion, string ip)
        {
            var entity = await _uow.Destinos.ObtenerPorIdAsync(desId)
                ?? throw new InvalidOperationException($"Destino {desId} no encontrado.");

            entity.DesEstado = 'I';
            entity.DesFechaEliminacion = DateTime.UtcNow;
            entity.DesUsuarioEliminacion = usuarioAccion;
            entity.DesIpEliminacion = ip;

            _uow.Destinos.Actualizar(entity);
            await _uow.SaveChangesAsync();
        }
    }
}
