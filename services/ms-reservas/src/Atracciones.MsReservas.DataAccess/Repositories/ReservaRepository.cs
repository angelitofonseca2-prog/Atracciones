using Atracciones.MsReservas.DataAccess.Context;
using Atracciones.MsReservas.DataAccess.Entities;
using Atracciones.MsReservas.DataManagement.Interfaces;
using Atracciones.MsReservas.DataManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsReservas.DataAccess.Repositories;

public sealed class ReservaRepository : IReservaRepository
{
    private readonly VentasDbContext _db;

    public ReservaRepository(VentasDbContext db) => _db = db;

    public async Task<ReservaDetalladaDto> CrearPendienteAsync(CrearReservaInternaDto dto, CancellationToken ct = default)
    {
        var revGuid = dto.RevGuidPreasignado ?? Guid.NewGuid();
        var codigo = string.IsNullOrWhiteSpace(dto.RevCodigo) ? GenerarCodigo() : dto.RevCodigo.Trim();

        var entity = new ReservaEntity
        {
            RevGuid = revGuid,
            CliGuid = dto.CliGuid,
            AtGuid = dto.AtGuid,
            HorGuid = dto.HorGuid,
            RevCodigo = codigo,
            RevEstado = 'P',
            RevSubtotal = dto.Subtotal,
            RevValorIva = dto.ValorIva,
            RevTotal = dto.Total,
            RevOrigenCanal = dto.OrigenCanal,
            RevFechaReservaUtc = DateTime.UtcNow,
            RevUsuarioIngreso = dto.UsuarioIngreso,
            RevIpIngreso = dto.IpIngreso,
            AtraccionNombreSnap = dto.AtraccionNombreSnap,
            HorFechaSnap = dto.HorFechaSnap,
            HorHoraInicioSnap = dto.HorHoraInicioSnap,
            HorHoraFinSnap = dto.HorHoraFinSnap,
        };

        foreach (var ln in dto.Lineas)
        {
            entity.Detalle.Add(new ReservaDetalleEntity
            {
                RdetGuid = Guid.NewGuid(),
                RevGuid = revGuid,
                TckGuid = ln.TckGuid,
                Cantidad = ln.Cantidad,
                PrecioUnit = ln.PrecioUnit,
                SubtotalLinea = ln.SubtotalLinea,
                TipoParticipante = ln.TipoParticipante,
            });
        }

        _db.Reservas.Add(entity);
        await _db.SaveChangesAsync(ct);

        return (await ObtenerPorGuidAsync(revGuid, ct))!;
    }

    public async Task<ReservaDetalladaDto?> ObtenerPorGuidAsync(Guid revGuid, CancellationToken ct = default)
    {
        var entity = await _db.Reservas.AsNoTracking()
            .Include(r => r.Detalle)
            .FirstOrDefaultAsync(r => r.RevGuid == revGuid, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<(IReadOnlyList<ReservaDetalladaDto> Items, int Total)> ListarPorClienteAsync(
        Guid cliGuid,
        int page,
        int limit,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 50);

        var q = _db.Reservas.AsNoTracking().Where(r => r.CliGuid == cliGuid);
        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(r => r.RevFechaReservaUtc)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Include(r => r.Detalle)
            .ToListAsync(ct);

        return (items.Select(Map).ToList(), total);
    }

    public async Task<(IReadOnlyList<ReservaAdminRowDto> Items, int Total)> ListarAdminAsync(
        int page,
        int limit,
        char? estado,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var q = _db.Reservas.AsNoTracking().AsQueryable();
        if (estado.HasValue)
            q = q.Where(r => r.RevEstado == estado.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(r => r.RevFechaReservaUtc)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(r => new ReservaAdminRowDto
            {
                RevGuid = r.RevGuid,
                RevCodigo = r.RevCodigo,
                CliGuid = r.CliGuid,
                Estado = r.RevEstado,
                Total = r.RevTotal,
                FechaReserva = r.RevFechaReservaUtc,
                AtraccionNombreSnap = r.AtraccionNombreSnap,
                HorFechaSnap = r.HorFechaSnap,
                HorHoraInicioSnap = r.HorHoraInicioSnap,
            })
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<ReservaDetalladaDto?> ConfirmarPagadaAsync(Guid revGuid, string usuario, string ip, CancellationToken ct = default)
    {
        var entity = await _db.Reservas.FirstOrDefaultAsync(r => r.RevGuid == revGuid, ct);
        if (entity is null)
            return null;
        if (entity.RevEstado != 'P')
            throw new InvalidOperationException($"La reserva no está pendiente de pago (estado actual: {entity.RevEstado}).");

        entity.RevEstado = 'A';
        await _db.SaveChangesAsync(ct);
        return await ObtenerPorGuidAsync(revGuid, ct);
    }

    public async Task<bool> AnularAsync(Guid revGuid, string motivo, string usuario, string ip, CancellationToken ct = default)
    {
        var entity = await _db.Reservas.FirstOrDefaultAsync(r => r.RevGuid == revGuid, ct);
        if (entity is null)
            return false;
        if (entity.RevEstado == 'C')
            return true;
        if (entity.RevEstado != 'P' && entity.RevEstado != 'A')
            throw new InvalidOperationException($"No se puede anular la reserva en estado {entity.RevEstado}.");

        entity.RevEstado = 'C';
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static ReservaDetalladaDto Map(ReservaEntity r) =>
        new()
        {
            RevGuid = r.RevGuid,
            RevCodigo = r.RevCodigo,
            CliGuid = r.CliGuid,
            AtGuid = r.AtGuid,
            HorGuid = r.HorGuid,
            Estado = r.RevEstado,
            Subtotal = r.RevSubtotal,
            ValorIva = r.RevValorIva,
            Total = r.RevTotal,
            Moneda = r.RevMoneda,
            OrigenCanal = r.RevOrigenCanal,
            RevFechaReservaUtc = r.RevFechaReservaUtc,
            AtraccionNombreSnap = r.AtraccionNombreSnap,
            HorFechaSnap = r.HorFechaSnap,
            HorHoraInicioSnap = r.HorHoraInicioSnap,
            HorHoraFinSnap = r.HorHoraFinSnap,
            Detalle = r.Detalle.OrderBy(d => d.TckGuid).Select(d => new ReservaDetalleDto
            {
                TckGuid = d.TckGuid,
                Cantidad = d.Cantidad,
                PrecioUnit = d.PrecioUnit,
                SubtotalLinea = d.SubtotalLinea,
                TipoParticipante = d.TipoParticipante,
            }).ToList(),
        };

    private static string GenerarCodigo()
    {
        var ts = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var rand = Random.Shared.Next(100, 999);
        return $"RES-{ts}{rand}";
    }
}
