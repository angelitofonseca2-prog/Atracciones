using Atracciones.MsOrquestador.Api.Options;
using Atracciones.MsOrquestador.Api.Security;
using Microsoft.Extensions.Options;

namespace Atracciones.MsOrquestador.Api.Hosted;

public sealed class JwksLoaderHostedService : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(5);

    private readonly JwksKeyStore _store;
    private readonly JwtValidationOptions _jwt;
    private readonly ILogger<JwksLoaderHostedService> _logger;

    public JwksLoaderHostedService(
        JwksKeyStore store,
        IOptions<JwtValidationOptions> jwt,
        ILogger<JwksLoaderHostedService> logger)
    {
        _store = store;
        _jwt = jwt.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_jwt.JwksUrl))
        {
            _logger.LogError("Jwt:JwksUrl es obligatorio.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _store.LoadAsync(_jwt.JwksUrl, stoppingToken);
                _logger.LogInformation("JWKS cargado desde {Url}", _jwt.JwksUrl);
                await Task.Delay(RefreshInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "No se pudo cargar JWKS desde {Url}; reintento en {Seconds}s.",
                    _jwt.JwksUrl,
                    RetryDelay.TotalSeconds);
                try
                {
                    await Task.Delay(RetryDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
