using Microservicio.Atracciones.Business.DTOs.Public.Resenias;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Public;
using Microservicio.Atracciones.Business.Mappers.Public;
using Microservicio.Atracciones.Business.Rules.Public;
using Microservicio.Atracciones.Business.Validators.Public;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Reservas;

namespace Microservicio.Atracciones.Business.Services.Public
{
    public class ReseniaPublicService : IReseniaPublicService
    {
        private readonly IReseniaDataService _reseniaService;
        private readonly IAtraccionDataService _atraccionService;
        private readonly IReservaDataService _reservaService;
        private readonly ITicketDataService _ticketService;
        private readonly IUsuarioDataService _usuarioService;
        private readonly IClienteDataService _clienteService;
        private readonly ReseniaRules _rules;

        public ReseniaPublicService(
            IReseniaDataService reseniaService,
            IAtraccionDataService atraccionService,
            IReservaDataService reservaService,
            ITicketDataService ticketService,
            IUsuarioDataService usuarioService,
            IClienteDataService clienteService,
            ReseniaRules rules)
        {
            _reseniaService = reseniaService;
            _atraccionService = atraccionService;
            _reservaService = reservaService;
            _ticketService = ticketService;
            _usuarioService = usuarioService;
            _clienteService = clienteService;
            _rules = rules;
        }

        public async Task<ReseniaResponse> CrearAsync(
            CrearReseniaRequest request, Guid usuGuid, string usuarioAccion, string ip)
        {
            ReseniaPublicValidator.Validar(request);
            var cliId = await ObtenerCliIdAsync(usuGuid);

            await _rules.ValidarPuedeResenarAsync(request.RevGuid, cliId);

            var reserva = await _reservaService.ObtenerPorGuidAsync(request.RevGuid)
                ?? throw new NotFoundException("Reserva", request.RevGuid);

            // Navegamos HorId → TckId → AtId para obtener la atracción de la reserva
            var horario = await _ticketService.ObtenerHorarioPorIdAsync(reserva.HorId)
                ?? throw new NotFoundException("Horario", reserva.HorId);

            var ticket = await _ticketService.ObtenerPorIdAsync(horario.TckId)
                ?? throw new NotFoundException("Ticket", horario.TckId);

            await _rules.ValidarNoHaResenadoAtraccionAsync(cliId, ticket.AtId);

            var reseniaModel = new ReseniaDataModel
            {
                AtId = ticket.AtId,
                RevId = reserva.RevId,
                RsnComentario = request.Comentario,
                RsnRating = request.Rating,
                RsnUsuarioCreacion = usuarioAccion,
                RsnIpCreacion = ip
            };

            await _reseniaService.CrearAsync(reseniaModel);

            var atraccion = await _atraccionService.ObtenerPorIdAsync(ticket.AtId);
            return ReseniaPublicMapper.ToResponse(reseniaModel, atraccion?.AtNombre ?? string.Empty);
        }

        private async Task<int> ObtenerCliIdAsync(Guid usuGuid)
        {
            var usuario = await _usuarioService.ObtenerPorGuidAsync(usuGuid)
                ?? throw new UnauthorizedBusinessException("El token no corresponde a un usuario activo.");

            var cliente = await _clienteService.ObtenerPorUsuarioIdAsync(usuario.UsuId)
                ?? throw new NotFoundException("Cliente asociado al usuario", usuGuid);

            return cliente.CliId;
        }

        public async Task<IReadOnlyList<ReseniaResponse>> ListarPorAtraccionAsync(Guid atGuid)
        {
            var atraccion = await _atraccionService.ObtenerPorGuidAsync(atGuid)
                ?? throw new NotFoundException("Atraccion", atGuid);

            var resenias = await _reseniaService.ListarPorAtraccionAsync(atraccion.AtId);
            return resenias.Select(r => ReseniaPublicMapper.ToResponse(r, atraccion.AtNombre)).ToList();
        }
    }
}
