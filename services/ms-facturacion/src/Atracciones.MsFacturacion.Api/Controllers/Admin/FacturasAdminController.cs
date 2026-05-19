using Atracciones.MsFacturacion.Api.Models;
using Atracciones.MsFacturacion.Api.Models.Common;
using Atracciones.MsFacturacion.DataManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsFacturacion.Api.Controllers.Admin;

[ApiController]
[Route("api/v2/admin/facturas")]
[Authorize(Policy = "SoloAdmin")]
public sealed class FacturasAdminController : ControllerBase
{
    private readonly IFacturaRepository _repo;

    public FacturasAdminController(IFacturaRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int page = 1, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var (rows, total) = await _repo.ListarAdminAsync(page, limit, ct);
        var data = rows.Select(r => new FacturaResponse
        {
            FacGuid = r.FacGuid.ToString("D"),
            FacNumero = r.FacNumero,
            RevCodigo = r.RevCodigoSnap,
            Total = r.Total,
            Moneda = r.Moneda,
            FechaEmision = r.FechaEmisionUtc,
            Estado = r.Estado.ToString(),
            NombreReceptor = string.Empty,
            CorreoReceptor = string.Empty,
        }).ToList();

        return Ok(new ApiListResponse<FacturaResponse>(data, total, page, limit));
    }

    [HttpGet("{guid:guid}")]
    public async Task<IActionResult> ObtenerPorGuid(Guid guid, CancellationToken ct = default)
    {
        var f = await _repo.ObtenerPorGuidAsync(guid, ct);
        if (f is null)
            return NotFound(new ApiErrorResponse { Status = 404, Error = "No encontrado", Details = new List<string> { "Factura no existe." }, Path = Request.Path.ToString() });

        var data = new FacturaResponse
        {
            FacGuid = f.FacGuid.ToString("D"),
            FacNumero = f.FacNumero,
            RevCodigo = f.RevCodigoSnap,
            Total = f.Total,
            Moneda = f.Moneda,
            FechaEmision = f.FechaEmisionUtc,
            Estado = f.Estado.ToString(),
            NombreReceptor = f.NombreReceptor,
            CorreoReceptor = f.CorreoReceptor,
        };
        return Ok(new ApiItemResponse<FacturaResponse>(data));
    }
}
