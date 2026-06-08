using Atracciones.MsReservas.DataAccess.Context;
using Atracciones.MsReservas.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsReservas.DataAccess.Repositories;

public interface IMarketplaceSeguimientoRepository
{
    Task CrearEnProcesoAsync(Guid seguimientoId, string correlationId, CancellationToken ct = default);
    Task ActualizarConfirmadaAsync(Guid seguimientoId, Guid revGuid, string revCodigo, CancellationToken ct = default);
    Task ActualizarRechazadaAsync(Guid seguimientoId, string motivo, CancellationToken ct = default);
    Task<MarketplaceReservaSeguimientoEntity?> ObtenerAsync(Guid seguimientoId, CancellationToken ct = default);
}

public sealed class MarketplaceSeguimientoRepository : IMarketplaceSeguimientoRepository
{
    private readonly VentasDbContext _db;

    public MarketplaceSeguimientoRepository(VentasDbContext db) => _db = db;

    public async Task CrearEnProcesoAsync(Guid seguimientoId, string correlationId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        _db.MarketplaceSeguimientos.Add(new MarketplaceReservaSeguimientoEntity
        {
            SeguimientoId = seguimientoId,
            Estado = "EN_PROCESO",
            CorrelationId = correlationId,
            CreatedUtc = now,
            UpdatedUtc = now,
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActualizarConfirmadaAsync(Guid seguimientoId, Guid revGuid, string revCodigo, CancellationToken ct = default)
    {
        var row = await _db.MarketplaceSeguimientos.FirstOrDefaultAsync(x => x.SeguimientoId == seguimientoId, ct)
            ?? throw new InvalidOperationException("Seguimiento no encontrado.");
        row.Estado = "CONFIRMADA";
        row.RevGuid = revGuid;
        row.RevCodigo = revCodigo;
        row.UpdatedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActualizarRechazadaAsync(Guid seguimientoId, string motivo, CancellationToken ct = default)
    {
        var row = await _db.MarketplaceSeguimientos.FirstOrDefaultAsync(x => x.SeguimientoId == seguimientoId, ct)
            ?? throw new InvalidOperationException("Seguimiento no encontrado.");
        row.Estado = "RECHAZADA";
        row.MotivoRechazo = motivo;
        row.UpdatedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public Task<MarketplaceReservaSeguimientoEntity?> ObtenerAsync(Guid seguimientoId, CancellationToken ct = default) =>
        _db.MarketplaceSeguimientos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.SeguimientoId == seguimientoId, ct);
}
