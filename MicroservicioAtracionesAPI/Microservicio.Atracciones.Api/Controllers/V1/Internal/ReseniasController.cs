using Asp.Versioning;
using Microservicio.Atracciones.Business.DTOs.Public.Resenias;
using Microservicio.Atracciones.Business.Interfaces.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microservicio.Atracciones.Api.Helpers;
using Microservicio.Atracciones.Api.Mappers.Public;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.Exceptions;

namespace Microservicio.Atracciones.Api.Controllers.V1.Internal
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/resenias")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public class ReseniasController : ControllerBase
    {
        private readonly IReseniaPublicService _service;

        public ReseniasController(IReseniaPublicService service)
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
        private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
        private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        // ----------------------------------------------------------------
        //  GET /api/v1/resenias?atraccionGuid={guid}
        //  Público — no requiere token
        // ----------------------------------------------------------------
        [HttpGet]
        [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<ReseniaResponse>>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> Listar([FromQuery] Guid atraccionGuid)
        {
            var resenias = await _service.ListarPorAtraccionAsync(atraccionGuid);
            var response = ReseniasApiMapper.ToListResponse(resenias);
            return Ok(response);
        }

        // ----------------------------------------------------------------
        //  POST /api/v1/resenias
        //  Requiere token de cliente — solo reseña reservas propias
        // ----------------------------------------------------------------
        [HttpPost]
        [Authorize(Policy = "ClienteAutenticado")]
        [EndpointName(EndpointNames.CrearResenia)]
        [ProducesResponseType(typeof(ApiItemResponse<ReseniaResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 401)]
        [ProducesResponseType(typeof(ApiErrorResponse), 403)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        public async Task<IActionResult> Crear([FromBody] CrearReseniaRequest request)
        {
            var resenia = await _service.CrearAsync(request, UsuGuidActual, UsuarioAccion, IpActual);
            var response = ReseniasApiMapper.ToResponse(resenia, statusCode: 201);
            return StatusCode(201, response);
        }
    }
}
