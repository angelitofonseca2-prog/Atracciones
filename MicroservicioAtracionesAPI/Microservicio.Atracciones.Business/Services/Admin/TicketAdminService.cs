using Microservicio.Atracciones.Business.DTOs.Admin.Tickets;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microservicio.Atracciones.Business.Mappers.Admin;
using Microservicio.Atracciones.Business.Rules.Admin;
using Microservicio.Atracciones.Business.Validators.Admin;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Reservas;

namespace Microservicio.Atracciones.Business.Services.Admin
{
    public class TicketAdminService : ITicketAdminService
    {
        private readonly ITicketDataService _ticketService;
        private readonly IAtraccionDataService _atraccionService;
        private readonly TicketRules _rules;

        public TicketAdminService(ITicketDataService ticketService, IAtraccionDataService atraccionService, TicketRules rules)
        {
            _ticketService = ticketService;
            _atraccionService = atraccionService;
            _rules = rules;
        }

        public async Task<IReadOnlyList<TicketResponse>> ListarTicketsAsync()
            => (await _ticketService.ListarAsync()).Select(t => TicketAdminMapper.ToResponse(t, string.Empty)).ToList();

        public async Task<TicketResponse> ObtenerTicketPorGuidAsync(Guid tckGuid)
        {
            var ticket = await _ticketService.ObtenerPorGuidAsync(tckGuid)
                ?? throw new NotFoundException("Ticket", tckGuid);

            return TicketAdminMapper.ToResponse(ticket, string.Empty);
        }

        public async Task<IReadOnlyList<TicketResponse>> ListarTicketsPorAtraccionAsync(Guid atGuid)
        {
            var atraccion = await _atraccionService.ObtenerPorGuidAsync(atGuid)
                ?? throw new NotFoundException("Atraccion", atGuid);

            return (await _ticketService.ListarPorAtraccionAsync(atraccion.AtId))
                .Select(t => TicketAdminMapper.ToResponse(t, atraccion.AtNombre))
                .ToList();
        }

        public async Task<TicketResponse> CrearTicketAsync(CrearTicketRequest request, string usuarioAccion, string ip)
        {
            TicketAdminValidator.ValidarCrear(request);
            await _rules.ValidarAtraccionExisteAsync(request.AtGuid);

            var atraccion = await _atraccionService.ObtenerPorGuidAsync(request.AtGuid)!;

            var model = new TicketDataModel
            {
                AtId = atraccion!.AtId,
                TckTitulo = request.Titulo,
                TckPrecio = request.Precio,
                TckTipoParticipante = request.TipoParticipante,
                TckCapacidadMaxima = request.CapacidadMaxima,
                TckCuposDisponibles = request.CuposDisponibles,
                TckEstado = 'A',
                TckUsuarioIngreso = usuarioAccion,
                TckIpIngreso = ip
            };

            await _ticketService.CrearAsync(model);
            return TicketAdminMapper.ToResponse(model, atraccion.AtNombre);
        }

        public async Task<TicketResponse> ActualizarTicketAsync(Guid tckGuid, ActualizarTicketRequest request, string usuarioAccion, string ip)
        {
            var model = await _ticketService.ObtenerPorGuidAsync(tckGuid)
                ?? throw new NotFoundException("Ticket", tckGuid);

            if (request.Titulo is not null) model.TckTitulo = request.Titulo;
            if (request.Precio is not null) model.TckPrecio = request.Precio.Value;
            if (request.Estado is not null) model.TckEstado = request.Estado.Value;
            if (request.CuposDisponibles is not null)
            {
                TicketRules.ValidarCupos(request.CuposDisponibles.Value, model.TckCapacidadMaxima);
                model.TckCuposDisponibles = request.CuposDisponibles.Value;
            }
            model.TckUsuarioMod = usuarioAccion;
            model.TckIpMod = ip;

            await _ticketService.ActualizarAsync(model);
            return TicketAdminMapper.ToResponse(model, string.Empty);
        }

        public async Task EliminarTicketAsync(Guid tckGuid, string usuarioAccion, string ip)
        {
            var model = await _ticketService.ObtenerPorGuidAsync(tckGuid)
                ?? throw new NotFoundException("Ticket", tckGuid);

            if (await _ticketService.TieneReservasActivasPorTicketAsync(model.TckId))
                throw new ConflictException("No se puede desactivar el ticket porque tiene reservas activas.");

            await _ticketService.EliminarLogicoAsync(model.TckId, usuarioAccion, ip);
        }

        public async Task<IReadOnlyList<HorarioResponse>> ListarHorariosAsync()
            => (await _ticketService.ListarHorariosAsync()).Select(TicketAdminMapper.ToHorarioResponse).ToList();

        public async Task<HorarioResponse> ObtenerHorarioPorGuidAsync(Guid horGuid)
        {
            var horario = await _ticketService.ObtenerHorarioPorGuidAsync(horGuid)
                ?? throw new NotFoundException("Horario", horGuid);

            return TicketAdminMapper.ToHorarioResponse(horario);
        }

        public async Task<IReadOnlyList<HorarioResponse>> ListarHorariosPorTicketAsync(Guid tckGuid)
        {
            var ticket = await _ticketService.ObtenerPorGuidAsync(tckGuid)
                ?? throw new NotFoundException("Ticket", tckGuid);

            return (await _ticketService.ListarHorariosPorTicketAsync(ticket.TckId))
                .Select(TicketAdminMapper.ToHorarioResponse)
                .ToList();
        }

        public async Task<IReadOnlyList<HorarioResponse>> ListarHorariosPorAtraccionAsync(Guid atGuid)
        {
            var atraccion = await _atraccionService.ObtenerPorGuidAsync(atGuid)
                ?? throw new NotFoundException("Atraccion", atGuid);

            return (await _ticketService.ListarHorariosAsync())
                .Where(h => h.AtId == atraccion.AtId)
                .Select(TicketAdminMapper.ToHorarioResponse)
                .ToList();
        }

        public async Task<HorarioResponse> CrearHorarioAsync(CrearHorarioRequest request, string usuarioAccion, string ip)
        {
            TicketAdminValidator.ValidarCrearHorario(request);
            await _rules.ValidarTicketExisteAsync(request.TckGuid);

            var ticket = await _ticketService.ObtenerPorGuidAsync(request.TckGuid)!;

            var horario = new HorarioDataModel
            {
                TckId = ticket!.TckId,
                HorFecha = request.Fecha,
                HorHoraInicio = request.HoraInicio,
                HorHoraFin = request.HoraFin,
                HorCuposDisponibles = request.CuposDisponibles,
                HorEstado = 'A',
                HorUsuarioIngreso = usuarioAccion,
                HorIpIngreso = ip
            };

            await _ticketService.CrearHorarioAsync(horario);
            
            var creado = await _ticketService.ObtenerHorarioPorGuidAsync(horario.HorGuid) ?? horario;
            return TicketAdminMapper.ToHorarioResponse(creado);
        }

        public async Task<HorarioResponse> ActualizarHorarioAsync(Guid horGuid, ActualizarHorarioRequest request, string usuarioAccion, string ip)
        {
            if (request.HoraInicio.HasValue && request.HoraFin.HasValue && request.HoraFin.Value <= request.HoraInicio.Value)
                throw new ValidationException(new[] { "La hora de fin debe ser posterior a la hora de inicio." });

            if (request.CuposDisponibles.HasValue && request.CuposDisponibles.Value < 0)
                throw new ValidationException(new[] { "Los cupos disponibles no pueden ser negativos." });

            var horario = await _ticketService.ObtenerHorarioPorGuidAsync(horGuid)
                ?? throw new NotFoundException("Horario", horGuid);

            var fecha = request.Fecha ?? horario.HorFecha;
            var horaInicio = request.HoraInicio ?? horario.HorHoraInicio;
            var horaFin = request.HoraFin ?? horario.HorHoraFin;

            if (horaFin.HasValue && horaFin.Value <= horaInicio)
                throw new ValidationException(new[] { "La hora de fin debe ser posterior a la hora de inicio." });

            if (request.Estado == 'I' && await _ticketService.TieneReservasActivasPorHorarioAsync(horario.HorId))
                throw new ConflictException("No se puede inactivar el horario porque tiene reservas activas.");

            horario.HorFecha = fecha;
            horario.HorHoraInicio = horaInicio;
            horario.HorHoraFin = horaFin;
            if (request.CuposDisponibles.HasValue)
                horario.HorCuposDisponibles = request.CuposDisponibles.Value;
            if (request.Estado.HasValue)
                horario.HorEstado = request.Estado.Value;
            horario.HorUsuarioMod = usuarioAccion;
            horario.HorIpMod = ip;

            await _ticketService.ActualizarHorarioAsync(horario);
            var actualizado = await _ticketService.ObtenerHorarioPorGuidAsync(horGuid) ?? horario;
            return TicketAdminMapper.ToHorarioResponse(actualizado);
        }

        public async Task EliminarHorarioAsync(Guid horGuid, string usuarioAccion, string ip)
        {
            var horario = await _ticketService.ObtenerHorarioPorGuidAsync(horGuid)
                ?? throw new NotFoundException("Horario", horGuid);

            if (await _ticketService.TieneReservasActivasPorHorarioAsync(horario.HorId))
                throw new ConflictException("No se puede desactivar el horario porque tiene reservas activas.");

            await _ticketService.EliminarHorarioLogicoAsync(horario.HorId, usuarioAccion, ip);
        }
    }
}
