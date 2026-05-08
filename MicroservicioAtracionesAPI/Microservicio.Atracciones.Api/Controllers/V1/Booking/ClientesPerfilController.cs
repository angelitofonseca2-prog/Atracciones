using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Public.Clientes;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Microservicio.Atracciones.Api.Controllers.V1.Booking
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/clientes/perfil")]
    [Authorize(Policy = "ClienteAutenticado")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public class ClientesPerfilController : ControllerBase
    {
        private readonly IClientePerfilService _service;

        public ClientesPerfilController(IClientePerfilService service)
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

        [HttpGet]
        [ProducesResponseType(typeof(PerfilClienteResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> Obtener()
            => Ok(await _service.ObtenerAsync(UsuGuidActual));

        [HttpPut]
        [ProducesResponseType(typeof(PerfilClienteResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> Actualizar([FromBody] ActualizarPerfilClienteRequest request)
            => Ok(await _service.ActualizarAsync(UsuGuidActual, request));
    }
}
