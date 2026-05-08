using Microservicio.Atracciones.Business.DTOs.Public.Reservas;
using Microservicio.Atracciones.Business.DTOs.Admin.Facturas;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Public;
using Microservicio.Atracciones.Business.Mappers.Admin;
using Microservicio.Atracciones.Business.Mappers.Public;
using Microservicio.Atracciones.Business.Rules.Public;
using Microservicio.Atracciones.Business.Validators.Public;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Reservas;
using Microservicio.Atracciones.DataManagement.Models.Common;
using Microservicio.Atracciones.DataManagement.Models.Clientes;
using Microservicio.Atracciones.DataManagement.Models.Facturacion;
using EmailAddressAttribute = System.ComponentModel.DataAnnotations.EmailAddressAttribute;

namespace Microservicio.Atracciones.Business.Services.Public
{
    public class ReservaPublicService : IReservaPublicService
    {
        private readonly IReservaDataService _reservaService;
        private readonly ITicketDataService _ticketService;
        private readonly IAtraccionDataService _atraccionService;  // #7: para obtener nombre real
        private readonly IUsuarioDataService _usuarioService;
        private readonly IClienteDataService _clienteService;
        private readonly IFacturaDataService _facturaService;
        private readonly ReservaRules _rules;
        private readonly IUnitOfWork _unitOfWork;

        public ReservaPublicService(
            IReservaDataService reservaService,
            ITicketDataService ticketService,
            IAtraccionDataService atraccionService,
            IUsuarioDataService usuarioService,
            IClienteDataService clienteService,
            IFacturaDataService facturaService,
            ReservaRules rules,
            IUnitOfWork unitOfWork)
        {
            _reservaService = reservaService;
            _ticketService = ticketService;
            _atraccionService = atraccionService;
            _usuarioService = usuarioService;
            _clienteService = clienteService;
            _facturaService = facturaService;
            _rules = rules;
            _unitOfWork = unitOfWork;
        }

        public async Task<ReservaResponse> CrearAsync(
            CrearReservaRequest request, Guid? usuGuid, string usuarioAccion, string ip)
        {
            ReservaPublicValidator.Validar(request);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var cliId = usuGuid.HasValue
                    ? await ObtenerCliIdAsync(usuGuid.Value)
                    : await ObtenerOCrearClienteInvitadoAsync(request.ClienteInvitado, usuarioAccion, ip);

                // 1. Validar y obtener tickets con precios actuales
                var lineas = request.Lineas.Select(l => (l.TckGuid, l.Cantidad)).ToList();
                var ticketsValidos = await _rules.ValidarYObtenerTicketsAsync(
                    request.HorGuid, request.AtGuid, lineas);

                // 2. Verificar cupos totales
                var totalPersonas = ticketsValidos.Sum(t => t.Cantidad);
                await _rules.ValidarDisponibilidadAsync(request.HorGuid, totalPersonas);

                // 3. Obtener horario
                var horario = await _ticketService.ObtenerHorarioPorGuidAsync(request.HorGuid)
                    ?? throw new NotFoundException("Horario", request.HorGuid);

                // 3.1 Obtener nombre real de la atracción y validar AtGuid
                var ticketModel  = await _ticketService.ObtenerPorIdAsync(horario.TckId);
                var atraccion    = ticketModel is not null
                    ? await _atraccionService.ObtenerPorIdAsync(ticketModel.AtId)
                    : null;
                    
                if (atraccion == null)
                    throw new ConflictException("La atracción del horario no fue encontrada.");
                    
                if (atraccion.AtGuid != request.AtGuid)
                    throw new ConflictException("El GUID de la atracción no coincide con el horario seleccionado.");
                    
                var atraccionNombre = atraccion.AtNombre;

                // 4. Calcular totales (IVA 15%)
                var (subtotal, iva, total) = ReservaRules.CalcularTotales(ticketsValidos);

                // 5. Construir modelo de reserva con líneas
                var reservaModel = new ReservaDataModel
                {
                    CliId = cliId,
                    HorId = horario.HorId,
                    RevCodigo = ReservaRules.GenerarCodigo(),
                    RevSubtotal = subtotal,
                    RevValorIva = iva,
                    RevTotal = total,
                    RevOrigenCanal = request.OrigenCanal,
                    RevUsuarioIngreso = usuarioAccion,
                    RevIpIngreso = ip,
                    Detalle = ticketsValidos.Select(t => new ReservaDetalleDataModel
                    {
                        TckId = t.Ticket.TckId,
                        RdetCantidad = t.Cantidad,
                        RdetPrecioUnit = t.Ticket.TckPrecio,   // precio congelado
                        RdetSubtotal = t.Ticket.TckPrecio * t.Cantidad,
                        TckTipoParticipante = t.Ticket.TckTipoParticipante,
                        RdetUsuarioIngreso = usuarioAccion,
                        RdetIpIngreso = ip
                    }).ToList()
                };

                await _reservaService.CrearAsync(reservaModel);

                // 6. Descontar cupos en HORARIO
                var horarioActualizado = new HorarioDataModel
                {
                    HorId = horario.HorId,
                    HorGuid = horario.HorGuid,
                    TckId = horario.TckId,
                    HorFecha = horario.HorFecha,
                    HorHoraInicio = horario.HorHoraInicio,
                    HorHoraFin = horario.HorHoraFin,
                    HorCuposDisponibles = horario.HorCuposDisponibles - totalPersonas,
                    HorEstado = horario.HorEstado,
                    HorUsuarioMod = usuarioAccion,
                    HorIpMod = ip
                };
                await _ticketService.ActualizarHorarioAsync(horarioActualizado);

                await transaction.CommitAsync();
                
                return ReservaPublicMapper.ToResponse(reservaModel, horario, atraccionNombre);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ReservaResponse> ObtenerPorGuidAsync(Guid revGuid, Guid usuGuid)
        {
            var cliId = await ObtenerCliIdAsync(usuGuid);
            var reserva = await _reservaService.ObtenerPorGuidAsync(revGuid)
                ?? throw new NotFoundException("Reserva", revGuid);

            if (reserva.CliId != cliId)
                throw new ForbiddenBusinessException("No tienes acceso a esta reserva.");

            // #1: resuelve el horario real usando el HorId que ya viene en el modelo
            var horario = await _ticketService.ObtenerHorarioPorIdAsync(reserva.HorId)
                ?? new HorarioDataModel();

            // #7: nombre real de la atracción
            var ticketModel = horario.TckId > 0
                ? await _ticketService.ObtenerPorIdAsync(horario.TckId)
                : null;
            var atraccion   = ticketModel is not null
                ? await _atraccionService.ObtenerPorIdAsync(ticketModel.AtId)
                : null;
            var atraccionNombre = atraccion?.AtNombre ?? "Atracción";

            return ReservaPublicMapper.ToResponse(reserva, horario, atraccionNombre);
        }

        public async Task<DataPagedResult<ReservaResponse>> ListarPorClienteAsync(Guid usuGuid, int page, int limit)
        {
            var cliId = await ObtenerCliIdAsync(usuGuid);
            var paged = await _reservaService.ListarPorClienteAsync(cliId, page, limit);
            var list = new List<ReservaResponse>();

            foreach(var reserva in paged.Items)
            {
                var horario = await _ticketService.ObtenerHorarioPorIdAsync(reserva.HorId) ?? new HorarioDataModel();
                var ticketModel = horario.TckId > 0 ? await _ticketService.ObtenerPorIdAsync(horario.TckId) : null;
                var atraccion = ticketModel is not null ? await _atraccionService.ObtenerPorIdAsync(ticketModel.AtId) : null;
                var atraccionNombre = atraccion?.AtNombre ?? "Atracción";
                list.Add(ReservaPublicMapper.ToResponse(reserva, horario, atraccionNombre));
            }
            return new DataPagedResult<ReservaResponse>(list, paged.TotalFiltrado, paged.TotalSinFiltros, paged.Page, paged.Limit);
        }

        public async Task CancelarAsync(Guid revGuid, CancelarReservaRequest request, Guid usuGuid, string usuarioAccion, string ip)
        {
            var cliId = await ObtenerCliIdAsync(usuGuid);
            var reserva = await _reservaService.ObtenerPorGuidAsync(revGuid)
                ?? throw new NotFoundException("Reserva", revGuid);

            if (reserva.CliId != cliId)
                throw new ForbiddenBusinessException("No puedes cancelar una reserva que no te pertenece.");

            if (reserva.RevEstado == 'C')
                throw new ConflictException("La reserva ya está cancelada.");

            if (reserva.RevEstado != 'A')
                throw new ConflictException("Solo se pueden cancelar reservas activas.");

            var motivo = string.IsNullOrWhiteSpace(request.Motivo)
                ? "Cancelada por el cliente."
                : request.Motivo.Trim();

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _reservaService.ActualizarEstadoAsync(reserva.RevId, 'C', motivo, usuarioAccion, ip);

                var horario = await _ticketService.ObtenerHorarioPorIdAsync(reserva.HorId);
                if (horario is not null)
                {
                    horario.HorCuposDisponibles += reserva.Detalle.Sum(d => d.RdetCantidad);
                    horario.HorUsuarioMod = usuarioAccion;
                    horario.HorIpMod = ip;
                    await _ticketService.ActualizarHorarioAsync(horario);
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<FacturaResponse> ConfirmarPagoAsync(
            Guid revGuid,
            ConfirmarPagoReservaRequest request,
            string usuarioAccion,
            string ip)
        {
            ValidarConfirmacionPago(request);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var reserva = await _reservaService.ObtenerPorGuidAsync(revGuid)
                    ?? throw new NotFoundException("Reserva", revGuid);

                if (reserva.RevEstado == 'C')
                    throw new ConflictException("La reserva está cancelada y no puede ser facturada");

                var facturaExistente = await _facturaService.ObtenerPorReservaAsync(reserva.RevId);
                if (facturaExistente is not null)
                    throw new ConflictException("Esta reserva ya fue facturada");

                var totalPersonas = reserva.Detalle.Sum(d => d.RdetCantidad);
                var horario = await _ticketService.ObtenerHorarioPorIdAsync(reserva.HorId)
                    ?? throw new NotFoundException("Horario", reserva.HorId);

                if (horario.HorCuposDisponibles < totalPersonas)
                    throw new ConflictException("Sin disponibilidad suficiente para confirmar la reserva");

                await _reservaService.ActualizarEstadoAsync(reserva.RevId, 'A', string.Empty, usuarioAccion, ip);

                horario.HorCuposDisponibles -= totalPersonas;
                horario.HorUsuarioMod = usuarioAccion;
                horario.HorIpMod = ip;
                await _ticketService.ActualizarHorarioAsync(horario);

                var factura = new FacturaDataModel
                {
                    RevId = reserva.RevId,
                    FacNumero = await _facturaService.GenerarNumeroSecuencialAsync(DateTime.UtcNow.Year),
                    FacTotal = reserva.RevTotal,
                    FacObservacion = request.Observacion?.Trim(),
                    FacOrigenCanal = reserva.RevOrigenCanal,
                    FacUsuarioIngreso = usuarioAccion,
                    FacIpIngreso = ip,
                    DatosFacturacion = new DatosFacturacionDataModel
                    {
                        DfacNombre = request.NombreReceptor.Trim(),
                        DfacApellido = request.ApellidoReceptor?.Trim(),
                        DfacCorreo = request.CorreoReceptor.Trim(),
                        DfacTelefono = request.TelefonoReceptor?.Trim()
                    }
                };

                await _facturaService.CrearAsync(factura);
                await transaction.CommitAsync();

                return FacturaAdminMapper.ToResponse(factura, reserva.RevCodigo);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<int> ObtenerCliIdAsync(Guid usuGuid)
        {
            var usuario = await _usuarioService.ObtenerPorGuidAsync(usuGuid)
                ?? throw new UnauthorizedBusinessException("El token no corresponde a un usuario activo.");

            var cliente = await _clienteService.ObtenerPorUsuarioIdAsync(usuario.UsuId)
                ?? throw new NotFoundException("Cliente asociado al usuario", usuGuid);

            return cliente.CliId;
        }

        private async Task<int> ObtenerOCrearClienteInvitadoAsync(
            ClienteInvitadoRequest? invitado,
            string usuarioAccion,
            string ip)
        {
            if (invitado is null)
                throw new ValidationException(new[] { "Debe enviar los datos del cliente invitado o autenticarse para reservar." });

            var errores = new List<string>();
            if (string.IsNullOrWhiteSpace(invitado.TipoIdentificacion))
                errores.Add("El tipo de identificación del cliente invitado es obligatorio.");
            if (string.IsNullOrWhiteSpace(invitado.NumeroIdentificacion))
                errores.Add("El número de identificación del cliente invitado es obligatorio.");
            if (string.IsNullOrWhiteSpace(invitado.Correo))
                errores.Add("El correo del cliente invitado es obligatorio.");
            if (string.IsNullOrWhiteSpace(invitado.RazonSocial) &&
                (string.IsNullOrWhiteSpace(invitado.Nombres) || string.IsNullOrWhiteSpace(invitado.Apellidos)))
                errores.Add("Debe enviar nombres y apellidos, o razón social, para el cliente invitado.");

            if (errores.Any())
                throw new ValidationException(errores);

            var existente = await _clienteService.ObtenerPorNumeroIdentificacionAsync(invitado.NumeroIdentificacion.Trim());
            if (existente is not null)
            {
                if (string.IsNullOrWhiteSpace(existente.CliCorreo) && !string.IsNullOrWhiteSpace(invitado.Correo))
                    existente.CliCorreo = invitado.Correo.Trim();
                if (string.IsNullOrWhiteSpace(existente.CliTelefono) && !string.IsNullOrWhiteSpace(invitado.Telefono))
                    existente.CliTelefono = invitado.Telefono.Trim();
                if (string.IsNullOrWhiteSpace(existente.CliDireccion) && !string.IsNullOrWhiteSpace(invitado.Direccion))
                    existente.CliDireccion = invitado.Direccion.Trim();

                await _clienteService.ActualizarAsync(existente);
                return existente.CliId;
            }

            var clienteModel = new ClienteDataModel
            {
                UsuId = null,
                CliTipoIdentificacion = invitado.TipoIdentificacion.Trim().ToUpperInvariant(),
                CliNumeroIdentificacion = invitado.NumeroIdentificacion.Trim(),
                CliNombres = invitado.Nombres?.Trim(),
                CliApellidos = invitado.Apellidos?.Trim(),
                CliRazonSocial = invitado.RazonSocial?.Trim(),
                CliCorreo = invitado.Correo.Trim(),
                CliTelefono = invitado.Telefono?.Trim(),
                CliDireccion = invitado.Direccion?.Trim(),
                CliEstado = 'A',
                CliUsuarioIngreso = usuarioAccion,
                CliIpIngreso = ip
            };

            await _clienteService.CrearAsync(clienteModel);
            return clienteModel.CliId;
        }

        private static void ValidarConfirmacionPago(ConfirmarPagoReservaRequest request)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(request.NombreReceptor))
                errores.Add("El nombre del receptor es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.CorreoReceptor))
            {
                errores.Add("El correo del receptor es obligatorio.");
            }
            else if (!new EmailAddressAttribute().IsValid(request.CorreoReceptor))
            {
                errores.Add("El correo del receptor no tiene un formato válido.");
            }

            if (errores.Any())
                throw new ValidationException(errores);
        }
    }
}
