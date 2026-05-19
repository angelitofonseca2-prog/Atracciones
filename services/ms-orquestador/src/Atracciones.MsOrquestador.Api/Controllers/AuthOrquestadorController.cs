using Atracciones.MsOrquestador.Api.Models.Common;
using Atracciones.MsOrquestador.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsOrquestador.Api.Controllers;

[ApiController]
[Route("api/v2/auth")]
[AllowAnonymous]
public sealed class AuthOrquestadorController : ControllerBase
{
    private readonly IRegistroOrquestacionService _registro;

    public AuthOrquestadorController(IRegistroOrquestacionService registro)
        => _registro = registro;

    private string CorrelationId =>
        HttpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault()
        ?? Guid.NewGuid().ToString("D");

    private string IpActual =>
        HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

    /// <summary>
    /// POST /api/v2/auth/registro
    /// SagaRegistroCliente: crea usuario en ms-identidad + perfil CRM (ClienteService en ms-reservas fusionado),
    /// compensa con EliminarUsuario si el alta de cliente falla, y devuelve el JWT.
    /// </summary>
    [HttpPost("registro")]
    public async Task<IActionResult> Registro([FromBody] RegistroApiRequest request, CancellationToken ct)
    {
        if (request is null)
            return BadRequest(new ApiErrorResponse
            {
                Status = 400, Message = "Cuerpo requerido",
                Path = HttpContext.Request.Path.ToString()
            });

        var dto = new RegistroOrquestadorDto
        {
            Login = request.Login?.Trim() ?? string.Empty,
            Password = request.Password ?? string.Empty,
            TipoIdentificacion = request.TipoIdentificacion ?? string.Empty,
            NumeroIdentificacion = request.NumeroIdentificacion?.Trim() ?? string.Empty,
            Nombres = request.Nombres?.Trim() ?? string.Empty,
            Apellidos = request.Apellidos?.Trim() ?? string.Empty,
            Correo = request.Correo?.Trim() ?? request.Login?.Trim() ?? string.Empty,
            Telefono = request.Telefono?.Trim(),
            IpCreador = IpActual,
        };

        var result = await _registro.RegistrarAsync(dto, CorrelationId, ct);

        return Ok(new ApiItemResponse<object>(new
        {
            token = result.Token,
            login = result.Login,
            roles = result.Roles,
        }, 200, "Cuenta creada exitosamente"));
    }
}

public sealed class RegistroApiRequest
{
    public string? Login { get; set; }
    public string? Password { get; set; }
    public string? TipoIdentificacion { get; set; }
    public string? NumeroIdentificacion { get; set; }
    public string? Nombres { get; set; }
    public string? Apellidos { get; set; }
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
}
