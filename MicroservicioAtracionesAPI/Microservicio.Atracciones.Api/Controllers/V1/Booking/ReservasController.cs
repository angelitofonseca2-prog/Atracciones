using Asp.Versioning;
using Microservicio.Atracciones.Business.DTOs.Admin.Facturas;
using Microservicio.Atracciones.Business.DTOs.Public.Reservas;
using Microservicio.Atracciones.Business.Interfaces.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microservicio.Atracciones.Api.Helpers;
using Microservicio.Atracciones.Api.Mappers.Public;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.Exceptions;

namespace Microservicio.Atracciones.Api.Controllers.V1.Booking
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/reservas")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public class ReservasController : ControllerBase
    {
        private readonly IReservaPublicService _service;

        public ReservasController(IReservaPublicService service)
            => _service = service;

        private Guid UsuGuidActual
        {
            get
            {
                var claim = User.FindFirstValue("usu_guid");
                if (!Guid.TryParse(claim, out var usuGuid))
                    throw new UnauthorizedBusinessException("El token no tiene un usuario válido.");

                return usuGuid;
            }
        }
        private Guid? UsuGuidOpcional
        {
            get
            {
                var claim = User.FindFirstValue("usu_guid");
                return Guid.TryParse(claim, out var usuGuid) ? usuGuid : null;
            }
        }
        private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
        private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        [HttpGet]
        [Authorize(Policy = "ClienteAutenticado")]
        [ProducesResponseType(typeof(ApiListResponse<ReservaResponse>), 200)]
        public async Task<IActionResult> ListarMisReservas([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var resultado = await _service.ListarPorClienteAsync(UsuGuidActual, page, limit);
            var response = new ApiListResponse<ReservaResponse>(resultado.Items, resultado.TotalFiltrado, page, limit);
            return Ok(response);
        }

        // ----------------------------------------------------------------
        //  POST /api/v1/reservas
        //  Crea reserva con cabecera + detalle en una sola transacción.
        //  Descuenta cupos en HORARIO. IVA 15%.
        // ----------------------------------------------------------------
        [HttpPost]
        [AllowAnonymous]
        [EndpointName(EndpointNames.CrearReserva)]
        [ProducesResponseType(typeof(ApiItemResponse<ReservaResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        public async Task<IActionResult> Crear([FromBody] CrearReservaRequest request)
        {
            var reserva = await _service.CrearAsync(request, UsuGuidOpcional, UsuarioAccion, IpActual);
            var response = ReservasApiMapper.ToResponse(reserva, statusCode: 201);
            return StatusCode(201, response);
        }

        [HttpPost("{guid:guid}/confirmar-pago")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiItemResponse<FacturaResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        public async Task<IActionResult> ConfirmarPago(Guid guid, [FromBody] ConfirmarPagoReservaRequest request)
        {
            var factura = await _service.ConfirmarPagoAsync(guid, request, UsuarioAccion, IpActual);
            return StatusCode(201, new ApiItemResponse<FacturaResponse>(
                factura,
                201,
                "Pago confirmado y factura generada"));
        }

        // ----------------------------------------------------------------
        //  GET /api/v1/reservas/{guid}
        //  El cliente solo puede ver sus propias reservas.
        // ----------------------------------------------------------------
        [HttpGet("{guid:guid}")]
        [Authorize(Policy = "ClienteAutenticado")]
        [EndpointName(EndpointNames.ObtenerReserva)]
        [ProducesResponseType(typeof(ApiItemResponse<ReservaResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 403)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> ObtenerPorGuid(Guid guid)
        {
            var reserva = await _service.ObtenerPorGuidAsync(guid, UsuGuidActual);
            var response = ReservasApiMapper.ToResponse(reserva);
            return Ok(response);
        }

        [HttpPut("{guid:guid}/cancelar")]
        [Authorize(Policy = "ClienteAutenticado")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResponse), 403)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        public async Task<IActionResult> Cancelar(Guid guid, [FromBody] CancelarReservaRequest request)
        {
            await _service.CancelarAsync(guid, request, UsuGuidActual, UsuarioAccion, IpActual);
            return NoContent();
        }

    }
}
