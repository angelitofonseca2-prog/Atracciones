using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Mappers.Clientes;
using Microservicio.Atracciones.DataManagement.Models.Clientes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Services
{
    public class ClienteDataService : IClienteDataService
    {
        private readonly IUnitOfWork _uow;

        public ClienteDataService(IUnitOfWork uow) => _uow = uow;

        public async Task<ClienteDataModel?> ObtenerPorIdAsync(int cliId)
        {
            var entity = await _uow.Clientes.ObtenerPorIdAsync(cliId);
            return ClienteDataMapper.ToDataModel(entity);
        }

        public async Task<ClienteDataModel?> ObtenerPorGuidAsync(Guid cliGuid)
        {
            var entity = await _uow.Clientes.ObtenerPorGuidAsync(cliGuid);
            return ClienteDataMapper.ToDataModel(entity);
        }

        public async Task<ClienteDataModel?> ObtenerPorUsuarioIdAsync(int usuId)
        {
            var entity = await _uow.Clientes.ObtenerPorUsuarioIdAsync(usuId);
            return ClienteDataMapper.ToDataModel(entity);
        }

        public async Task<ClienteDataModel?> ObtenerPorNumeroIdentificacionAsync(string numero)
        {
            var entity = await _uow.Clientes.ObtenerPorNumeroIdentificacionAsync(numero);
            return ClienteDataMapper.ToDataModel(entity);
        }

        public async Task<IReadOnlyList<ClienteDataModel>> ListarAsync()
        {
            var entities = await _uow.Clientes.ListarAsync();
            return entities.Select(e => ClienteDataMapper.ToDataModel(e)!).ToList();
        }

        public async Task CrearAsync(ClienteDataModel model)
        {
            var entity = ClienteDataMapper.ToNewEntity(model);
            await _uow.Clientes.AgregarAsync(entity);
            await _uow.SaveChangesAsync();

            model.CliId = entity.CliId;
            model.CliGuid = entity.CliGuid;
        }

        public async Task ActualizarAsync(ClienteDataModel model)
        {
            var entity = await _uow.Clientes.ObtenerPorIdAsync(model.CliId)
                ?? throw new InvalidOperationException($"Cliente {model.CliId} no encontrado.");

            ClienteDataMapper.ApplyToEntity(model, entity);
            _uow.Clientes.Actualizar(entity);
            await _uow.SaveChangesAsync();
        }

        public async Task EliminarLogicoAsync(int cliId, string usuarioAccion, string ip)
        {
            var entity = await _uow.Clientes.ObtenerPorIdAsync(cliId)
                ?? throw new InvalidOperationException($"Cliente {cliId} no encontrado.");

            entity.CliEstado = 'I';
            entity.CliFechaEliminacion = DateTime.UtcNow;
            entity.CliUsuarioEliminacion = usuarioAccion;
            entity.CliIpEliminacion = ip;

            _uow.Clientes.Actualizar(entity);
            await _uow.SaveChangesAsync();
        }

        public async Task<bool> ExisteIdentificacionAsync(string numero)
        {
            var entity = await _uow.Clientes.ObtenerPorNumeroIdentificacionAsync(numero);
            return entity is not null;
        }
    }
}
