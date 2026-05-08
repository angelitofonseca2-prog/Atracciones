using Microservicio.Atracciones.DataAccess.Queries;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Mappers.Reservas;
using Microservicio.Atracciones.DataManagement.Mappers.Facturacion;
using Microservicio.Atracciones.DataManagement.Mappers.Auditoria;
using Microservicio.Atracciones.DataManagement.Models.Reservas;
using Microservicio.Atracciones.DataManagement.Models.Facturacion;
using Microservicio.Atracciones.DataManagement.Models.Auditoria;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.DataManagement.Services
{
    public class TicketDataService : ITicketDataService
    {
        private readonly IUnitOfWork _uow;
        private readonly TicketQueryRepository _queryRepo;

        public TicketDataService(IUnitOfWork uow, TicketQueryRepository queryRepo)
        {
            _uow = uow;
            _queryRepo = queryRepo;
        }

        public async Task<TicketDataModel?> ObtenerPorIdAsync(int tckId)
            => TicketDataMapper.ToDataModel(await _uow.Tickets.ObtenerPorIdAsync(tckId));

        public async Task<TicketDataModel?> ObtenerPorGuidAsync(Guid tckGuid)
            => TicketDataMapper.ToDataModel(await _uow.Tickets.ObtenerPorGuidAsync(tckGuid));

        public async Task<IReadOnlyList<TicketDataModel>> ListarAsync()
        {
            var entities = await _uow.Tickets.ListarActivosAsync();
            return entities.Select(e => TicketDataMapper.ToDataModel(e)!).ToList();
        }

        public async Task<IReadOnlyList<TicketDataModel>> ListarPorAtraccionAsync(int atId)
        {
            var entities = await _uow.Tickets.ListarPorAtraccionAsync(atId);
            return entities.Select(e => TicketDataMapper.ToDataModel(e)!).ToList();
        }
        public async Task<HorarioDataModel?> ObtenerHorarioPorIdAsync(int horId)
            => TicketDataMapper.ToHorarioDataModel(await _uow.Tickets.ObtenerHorarioPorIdAsync(horId));

        public async Task CrearAsync(TicketDataModel model)
        {
            var entity = TicketDataMapper.ToNewEntity(model);
            await _uow.Tickets.AgregarAsync(entity);
            await _uow.SaveChangesAsync();

            model.TckId = entity.TckId;
            model.TckGuid = entity.TckGuid;
        }

        public async Task ActualizarAsync(TicketDataModel model)
        {
            var entity = await _uow.Tickets.ObtenerPorIdAsync(model.TckId)
                ?? throw new InvalidOperationException($"Ticket {model.TckId} no encontrado.");

            TicketDataMapper.ApplyToEntity(model, entity);
            _uow.Tickets.Actualizar(entity);
            await _uow.SaveChangesAsync();
        }

        public async Task EliminarLogicoAsync(int tckId, string usuarioAccion, string ip)
        {
            var entity = await _uow.Tickets.ObtenerPorIdAsync(tckId)
                ?? throw new InvalidOperationException($"Ticket {tckId} no encontrado.");

            entity.TckEstado = 'I';
            entity.TckFechaEliminacion = DateTime.UtcNow;
            entity.TckUsuarioEliminacion = usuarioAccion;
            entity.TckIpEliminacion = ip;

            _uow.Tickets.Actualizar(entity);
            await _uow.SaveChangesAsync();
        }

        public async Task<HorarioDataModel?> ObtenerHorarioPorGuidAsync(Guid horGuid)
            => TicketDataMapper.ToHorarioDataModel(await _uow.Tickets.ObtenerHorarioPorGuidAsync(horGuid));

        public async Task<IReadOnlyList<HorarioDataModel>> ListarHorariosAsync()
        {
            var entities = await _uow.Tickets.ListarHorariosActivosAsync();
            return entities.Select(e => TicketDataMapper.ToHorarioDataModel(e)!).ToList();
        }

        public async Task<IReadOnlyList<HorarioDataModel>> ListarHorariosPorTicketAsync(int tckId)
        {
            var entities = await _uow.Tickets.ListarHorariosPorTicketAsync(tckId);
            return entities.Select(e => TicketDataMapper.ToHorarioDataModel(e)!).ToList();
        }

        public async Task<IReadOnlyList<HorarioDataModel>> ListarHorariosPorAtraccionAsync(int atId, int diasAdelante = 7)
        {
            var entities = await _queryRepo.ObtenerHorariosPorAtraccionAsync(atId, diasAdelante);
            return entities.Select(e => TicketDataMapper.ToHorarioDataModel(e)!).ToList();
        }

        public async Task CrearHorarioAsync(HorarioDataModel model)
        {
            var entity = TicketDataMapper.ToNewHorarioEntity(model);
            await _uow.Tickets.AgregarHorarioAsync(entity);
            await _uow.SaveChangesAsync();

            model.HorId = entity.HorId;
            model.HorGuid = entity.HorGuid;
        }

        public async Task ActualizarHorarioAsync(HorarioDataModel model)
        {
            var entity = await _uow.Tickets.ObtenerHorarioPorIdAsync(model.HorId)
                ?? throw new InvalidOperationException($"Horario {model.HorId} no encontrado.");

            entity.HorFecha = model.HorFecha;
            entity.HorHoraInicio = model.HorHoraInicio;
            entity.HorHoraFin = model.HorHoraFin;
            entity.HorCuposDisponibles = model.HorCuposDisponibles;
            entity.HorEstado = model.HorEstado;
            entity.HorFechaMod = DateTime.UtcNow;
            entity.HorUsuarioMod = model.HorUsuarioMod;
            entity.HorIpMod = model.HorIpMod;

            _uow.Tickets.ActualizarHorario(entity);
            await _uow.SaveChangesAsync();
        }

        public async Task EliminarHorarioLogicoAsync(int horId, string usuarioAccion, string ip)
        {
            var entity = await _uow.Tickets.ObtenerHorarioPorIdAsync(horId)
                ?? throw new InvalidOperationException($"Horario {horId} no encontrado.");

            entity.HorEstado = 'I';
            entity.HorFechaEliminacion = DateTime.UtcNow;
            entity.HorUsuarioEliminacion = usuarioAccion;
            entity.HorIpEliminacion = ip;

            _uow.Tickets.ActualizarHorario(entity);
            await _uow.SaveChangesAsync();
        }

        public Task<bool> TieneReservasActivasPorTicketAsync(int tckId)
            => _queryRepo.TieneReservasActivasPorTicketAsync(tckId);

        public Task<bool> TieneReservasActivasPorHorarioAsync(int horId)
            => _queryRepo.TieneReservasActivasPorHorarioAsync(horId);

        public Task<(bool DisponibleHoy, DateOnly? ProximaFecha, int? CuposProximos)>
            ObtenerDisponibilidadAsync(int atId)
            => _queryRepo.ObtenerDisponibilidadAsync(atId);

        // #4: Batch — delega al query repo que hace una sola query con WHERE IN
        public Task<Dictionary<int, (bool DisponibleHoy, DateOnly? ProximaFecha, int? CuposProximos)>>
            ObtenerDisponibilidadBatchAsync(IEnumerable<int> atIds)
            => _queryRepo.ObtenerDisponibilidadBatchAsync(atIds);

        public Task<(bool HayCupos, int CuposActuales)>
            VerificarCuposAsync(int horId, int cantidadSolicitada)
            => _queryRepo.VerificarCuposAsync(horId, cantidadSolicitada);
    }
}
