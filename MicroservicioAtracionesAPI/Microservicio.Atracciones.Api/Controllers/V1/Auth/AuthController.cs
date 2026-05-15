using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Clientes;
using Microservicio.Atracciones.Business.DTOs.Auth;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microservicio.Atracciones.Business.Interfaces.Integration;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio.Atracciones.Api.Controllers.V1.Auth
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IClienteAdminService _clienteAdminService;
        private readonly IUsuarioDataService _usuarioData;
        private readonly IIdentidadUsuarioSyncPublisher _identidadSync;

        public AuthController(
            IClienteAdminService clienteAdminService,
            IUsuarioDataService usuarioData,
            IIdentidadUsuarioSyncPublisher identidadSync)
        {
            _clienteAdminService = clienteAdminService;
            _usuarioData = usuarioData;
            _identidadSync = identidadSync;
        }

        /// <summary>
        /// El login lo atiende ms-identidad. Llamar al monolito directamente ya no emite tokens.
        /// </summary>
        [HttpPost("login")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult LoginNoDisponible()
        {
            return StatusCode(501, new ApiErrorResponse
            {
                Status = 501,
                Error = "Login no implementado en el monolito",
                Details = new List<string> { "Use POST /api/v1/auth/login a través del API Gateway (ms-identidad)." },
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Path = HttpContext.Request.Path.ToString(),
            });
        }

        [HttpPost("registro")]
        [ProducesResponseType(typeof(ApiItemResponse<LoginResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 503)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> Registro([FromBody] RegistroClienteRequest request)
        {
            var crearRequest = new CrearClienteRequest
            {
                Login = request.Login,
                Password = request.Password,
                TipoIdentificacion = request.TipoIdentificacion,
                NumeroIdentificacion = request.NumeroIdentificacion,
                Nombres = request.Nombres,
                Apellidos = request.Apellidos,
                Correo = request.Correo,
                Telefono = request.Telefono
            };

            await _clienteAdminService.CrearAsync(
                crearRequest,
                "publico",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0");

            var usuario = await _usuarioData.ObtenerPorLoginAsync(request.Login.Trim());
            if (usuario is null)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Status = 500,
                    Error = "Error interno",
                    Details = new List<string> { "No se pudo cargar el usuario recién creado." },
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Path = HttpContext.Request.Path.ToString(),
                });
            }

            var roles = usuario.Roles.Select(r => r.RolDescripcion.ToUpperInvariant()).ToList();
            var tokenResult = await _identidadSync.SincronizarYObtenerTokenAsync(
                new IdentidadUsuarioEspejo(
                    usuario.UsuId,
                    usuario.UsuGuid,
                    usuario.UsuLogin,
                    usuario.UsuPasswordHash,
                    usuario.CliId,
                    roles),
                HttpContext.RequestAborted);

            if (tokenResult is null)
            {
                return StatusCode(503, new ApiErrorResponse
                {
                    Status = 503,
                    Error = "Servicio de identidad no disponible",
                    Details = new List<string>
                    {
                        "ms-identidad no respondió o está deshabilitado. Verifica Identidad:BaseUrl y que el servicio esté en ejecución.",
                    },
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Path = HttpContext.Request.Path.ToString(),
                });
            }

            var response = new ApiItemResponse<LoginResponse>(new LoginResponse
            {
                Token = tokenResult.Token,
                Expiracion = tokenResult.Expiracion,
                Login = tokenResult.Login,
                Roles = tokenResult.Roles.ToList(),
            }, 201);

            return StatusCode(201, response);
        }
    }
}
