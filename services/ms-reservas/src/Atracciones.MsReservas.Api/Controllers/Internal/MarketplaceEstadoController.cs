using Atracciones.MsReservas.DataAccess.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsReservas.Api.Controllers.Internal;

[ApiController]
[Route("internal/v1/marketplace")]
public sealed class MarketplaceEstadoController : ControllerBase
{
    private readonly IMarketplaceSeguimientoRepository _seguimiento;

    public MarketplaceEstadoController(IMarketplaceSeguimientoRepository seguimiento) =>
        _seguimiento = seguimiento;

    [HttpGet("reservas/{seguimientoId:guid}/estado")]
    public async Task<IActionResult> ObtenerEstado(Guid seguimientoId, CancellationToken ct)
    {
        var row = await _seguimiento.ObtenerAsync(seguimientoId, ct);
        if (row is null)
            return NotFound(new { status = 404, message = "Seguimiento no encontrado." });

        return Ok(new
        {
            status = 200,
            data = new
            {
                seguimiento_id = row.SeguimientoId,
                rev_guid = row.RevGuid,
                rev_codigo = row.RevCodigo,
                estado = row.Estado,
                motivo_rechazo = row.MotivoRechazo,
                correlation_id = row.CorrelationId,
                updated_utc = row.UpdatedUtc,
            },
        });
    }
}
