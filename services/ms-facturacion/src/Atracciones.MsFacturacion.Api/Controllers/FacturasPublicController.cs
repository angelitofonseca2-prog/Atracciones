using System.Security.Claims;
using Atracciones.MsFacturacion.Api.Models;
using Atracciones.MsFacturacion.Api.Models.Common;
using Atracciones.MsFacturacion.DataManagement.Interfaces;
using Atracciones.MsFacturacion.DataManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsFacturacion.Api.Controllers;

[ApiController]
[Route("api/v2/facturas")]
[Authorize(Policy = "ClienteAutenticado")]
public sealed class FacturasPublicController : ControllerBase
{
    private readonly IFacturaRepository _repo;

    public FacturasPublicController(IFacturaRepository repo) => _repo = repo;

    private Guid CliGuidActual
    {
        get
        {
            var claim = User.FindFirstValue("usu_guid")
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(claim, out var g))
                throw new UnauthorizedAccessException("El token no tiene un usuario válido.");
            return g;
        }
    }

    [HttpGet("mis-facturas")]
    public async Task<IActionResult> MisFacturas([FromQuery] int page = 1, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var cli = CliGuidActual;
        var (items, total) = await _repo.ListarPorClienteAsync(cli, page, limit, ct);
        var data = items.Select(Map).ToList();
        return Ok(new ApiListResponse<FacturaResponse>(data, total, page, limit));
    }

    private static FacturaResponse Map(FacturaEmitidaDto f) =>
        new()
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
}
