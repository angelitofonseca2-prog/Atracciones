using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Atracciones.Contracts.Clientes.V1;
using Atracciones.Contracts.Identidad.V1;
using Atracciones.MsOrquestador.Business.Exceptions;
using Atracciones.MsOrquestador.Business.Integration;
using Atracciones.MsOrquestador.Business.Options;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atracciones.MsOrquestador.Business.Services;

public sealed class RegistroOrquestacionAppService : IRegistroOrquestacionService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly UsuarioService.UsuarioServiceClient _idn;
    private readonly ClienteService.ClienteServiceClient _cli;
    private readonly IHttpClientFactory _httpFactory;
    private readonly AuditoriaBestEffortPublisher _audit;
    private readonly string _identidadHttpBase;
    private readonly ILogger<RegistroOrquestacionAppService> _logger;

    public RegistroOrquestacionAppService(
        UsuarioService.UsuarioServiceClient idn,
        ClienteService.ClienteServiceClient cli,
        IHttpClientFactory httpFactory,
        AuditoriaBestEffortPublisher audit,
        IOptions<GrpcClientsOptions> opts,
        ILogger<RegistroOrquestacionAppService> logger)
    {
        _idn = idn;
        _cli = cli;
        _httpFactory = httpFactory;
        _audit = audit;
        _identidadHttpBase = opts.Value.IdentidadHttp.TrimEnd('/');
        _logger = logger;
    }

    public async Task<RegistroOrquestadorResultDto> RegistrarAsync(
        RegistroOrquestadorDto dto,
        string correlationId,
        CancellationToken ct)
    {
        // Paso 1 — crear usuario en ms-identidad (gRPC)
        UsuarioReply usuReply;
        try
        {
            usuReply = await _idn.CrearUsuarioAsync(new CrearUsuarioRequest
            {
                Login = dto.Login.Trim(),
                PasswordPlain = dto.Password,
                CreadoPor = "registro-publico",
                IpCreador = dto.IpCreador,
            }, cancellationToken: ct);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.AlreadyExists)
        {
            throw new ConflictOrchestadorException("Ya existe un usuario registrado con ese correo.");
        }

        var usuGuid = usuReply.UsuGuid;

        // Paso 2 — crear perfil CRM vía ClienteService en ms-reservas (gRPC fusionado); compensar si falla
        try
        {
            await _cli.CrearClienteAsync(new CrearClienteRequest
            {
                UsuGuid = usuGuid,
                TipoIdentificacion = dto.TipoIdentificacion,
                NumeroIdentificacion = dto.NumeroIdentificacion,
                Nombres = dto.Nombres.Trim(),
                Apellidos = dto.Apellidos.Trim(),
                Correo = dto.Correo.Trim(),
                Telefono = dto.Telefono?.Trim() ?? string.Empty,
                CreadoPor = "registro-publico",
                IpCreador = dto.IpCreador,
            }, cancellationToken: ct);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.AlreadyExists)
        {
            await CompensarEliminarUsuario(usuGuid);
            throw new ConflictOrchestadorException("Ya existe un cliente con ese número de identificación.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear perfil de cliente. Compensando usuario {Guid}", usuGuid);
            await CompensarEliminarUsuario(usuGuid);
            throw;
        }

        // Paso 3 — obtener JWT llamando al login HTTP de ms-identidad
        var result = await ObtenerTokenAsync(dto.Login, dto.Password, ct);

        // Auditoría best-effort
        _audit.Registrar(
            "REGISTRO_CLIENTE_COMPLETADO", correlationId,
            $"{{\"login\":\"{dto.Login}\",\"usu_guid\":\"{usuGuid}\"}}");

        return result;
    }

    private async Task CompensarEliminarUsuario(string usuGuid)
    {
        try
        {
            await _idn.EliminarUsuarioAsync(new EliminarUsuarioRequest { UsuGuid = usuGuid });
            _logger.LogInformation("Compensación: usuario {Guid} eliminado tras fallo en registro", usuGuid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo compensar (EliminarUsuario {Guid})", usuGuid);
        }
    }

    private async Task<RegistroOrquestadorResultDto> ObtenerTokenAsync(
        string login, string password, CancellationToken ct)
    {
        using var http = _httpFactory.CreateClient("identidad");
        var body = JsonSerializer.Serialize(new { login, password }, JsonOpts);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var resp = await http.PostAsync($"{_identidadHttpBase}/api/v2/auth/login", content, ct);

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("ms-identidad login post-registro respondió {Status}", resp.StatusCode);
            throw new InvalidOperationException(
                "Registro completado, pero no se pudo emitir el token. Inicia sesión manualmente.");
        }

        using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var dataEl = doc.RootElement.GetProperty("data");
        var token = dataEl.GetProperty("token").GetString() ?? string.Empty;
        var loginVal = dataEl.TryGetProperty("login", out var lEl) ? lEl.GetString() ?? login : login;
        var roles = dataEl.TryGetProperty("roles", out var rEl)
            ? rEl.EnumerateArray().Select(e => e.GetString() ?? string.Empty).ToArray()
            : Array.Empty<string>();

        return new RegistroOrquestadorResultDto { Token = token, Login = loginVal, Roles = roles };
    }
}
