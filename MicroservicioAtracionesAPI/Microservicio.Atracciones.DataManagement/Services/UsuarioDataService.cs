using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Mappers.Seguridad;
using Microservicio.Atracciones.DataManagement.Models.Seguridad;
using Microservicio.Atracciones.DataAccess.Context;
using Microservicio.Atracciones.DataAccess.Entities.Seguridad;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Services
{
    public class UsuarioDataService : IUsuarioDataService
    {
        private readonly IUnitOfWork _uow;
        private readonly AtraccionesDbContext _context;

        public UsuarioDataService(IUnitOfWork uow, AtraccionesDbContext context)
        {
            _uow = uow;
            _context = context;
        }

        public async Task<UsuarioDataModel?> ObtenerPorIdAsync(int usuId)
        {
            var entity = await _uow.Usuarios.ObtenerPorIdAsync(usuId);
            return UsuarioDataMapper.ToDataModel(entity);
        }

        public async Task<UsuarioDataModel?> ObtenerPorLoginAsync(string login)
        {
            var entity = await _uow.Usuarios.ObtenerPorLoginAsync(login);
            return UsuarioDataMapper.ToDataModel(entity);
        }

        public async Task<UsuarioDataModel?> ObtenerPorGuidAsync(Guid usuGuid)
        {
            var entity = await _uow.Usuarios.ObtenerPorGuidAsync(usuGuid);
            return UsuarioDataMapper.ToDataModel(entity);
        }

        public async Task<IReadOnlyList<UsuarioDataModel>> ListarAsync()
        {
            var entities = await _uow.Usuarios.ListarAsync();
            return entities.Select(e => UsuarioDataMapper.ToDataModel(e)!).ToList();
        }

        public async Task<IReadOnlyList<RolDataModel>> ObtenerRolesPorUsuarioAsync(int usuId)
        {
            var entity = await _uow.Usuarios.ObtenerPorIdAsync(usuId);
            if (entity is null) return new List<RolDataModel>();

            return entity.UsuariosRoles
                         .Where(ur => ur.UsuRolEstado == 'A')
                         .Select(ur => RolDataMapper.ToDataModel(ur.RolEntity)!)
                         .ToList();
        }

        public async Task CrearAsync(UsuarioDataModel model)
        {
            var entity = UsuarioDataMapper.ToNewEntity(model);

            var rolesSolicitados = model.Roles
                .Select(r => r.RolDescripcion.Trim().ToUpperInvariant())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct()
                .ToList();

            if (rolesSolicitados.Any())
            {
                var roles = await _context.Roles
                    .Where(r => r.RolEstado == 'A' && rolesSolicitados.Contains(r.RolDescripcion.ToUpper()))
                    .ToListAsync();

                var faltantes = rolesSolicitados
                    .Except(roles.Select(r => r.RolDescripcion.ToUpperInvariant()))
                    .ToList();

                if (faltantes.Any())
                    throw new InvalidOperationException($"Roles no encontrados en la base: {string.Join(", ", faltantes)}.");

                foreach (var rol in roles)
                {
                    entity.UsuariosRoles.Add(new UsuarioRolEntity
                    {
                        RolId = rol.RolId,
                        UsuRolEstado = 'A'
                    });
                }
            }

            await _uow.Usuarios.AgregarAsync(entity);
            await _uow.SaveChangesAsync();

            // Sincroniza el Id generado de vuelta al model
            model.UsuId = entity.UsuId;
            model.UsuGuid = entity.UsuGuid;
        }

        public async Task ActualizarAsync(UsuarioDataModel model)
        {
            var entity = await _uow.Usuarios.ObtenerPorIdAsync(model.UsuId)
                ?? throw new InvalidOperationException($"Usuario {model.UsuId} no encontrado.");

            UsuarioDataMapper.ApplyToEntity(model, entity);
            _uow.Usuarios.Actualizar(entity);
            await _uow.SaveChangesAsync();
        }

        public async Task EliminarLogicoAsync(int usuId, string usuarioAccion, string ip)
        {
            var entity = await _uow.Usuarios.ObtenerPorIdAsync(usuId)
                ?? throw new InvalidOperationException($"Usuario {usuId} no encontrado.");

            entity.UsuEstado = 'I';
            entity.UsuFechaEliminacion = DateTime.UtcNow;
            entity.UsuUsuarioEliminacion = usuarioAccion;
            entity.UsuIpEliminacion = ip;

            _uow.Usuarios.Actualizar(entity);
            await _uow.SaveChangesAsync();
        }

        public async Task<bool> ExisteLoginAsync(string login)
        {
            var entity = await _uow.Usuarios.ObtenerPorLoginAsync(login);
            return entity is not null;
        }
    }
}
