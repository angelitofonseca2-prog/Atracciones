using Microservicio.Atracciones.Api.Models.Integration;
using Microservicio.Atracciones.Api.Models.Settings;
using Microservicio.Atracciones.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Microservicio.Atracciones.Api.Controllers.Internal;

[AllowAnonymous]
[ApiController]
[Route("internal/v1/catalogos")]
public sealed class CatalogMirrorController : ControllerBase
{
    private readonly ICatalogMirrorApplicator _applicator;
    private readonly CatalogMirrorIngressSettings _opts;

    public CatalogMirrorController(
        ICatalogMirrorApplicator applicator,
        IOptions<CatalogMirrorIngressSettings> opts)
    {
        _applicator = applicator;
        _opts = opts.Value;
    }

    [HttpPost("mirror")]
    public async Task<IActionResult> Mirror([FromBody] CatalogMirrorIngressPayload body, CancellationToken cancellationToken)
    {
        if (!_opts.Enabled || string.IsNullOrWhiteSpace(_opts.ApiKey) ||
            !Request.Headers.TryGetValue("X-Monolith-Sync-Key", out var sent) ||
            sent.Count != 1 ||
            !string.Equals(sent[0], _opts.ApiKey, StringComparison.Ordinal))
            return Unauthorized();

        await _applicator.ApplyAsync(body, cancellationToken);
        return Ok(new { ok = true });
    }
}
