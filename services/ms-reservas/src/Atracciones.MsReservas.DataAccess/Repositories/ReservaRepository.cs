using Atracciones.MsReservas.DataAccess.Context;
using Atracciones.MsReservas.DataAccess.Entities;
using Atracciones.MsReservas.DataManagement.Interfaces;
using Atracciones.MsReservas.DataManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

        var q = _db.Reservas.AsNoTracking()
            .Where(r => r.CliGuid == cliGuid && (r.RevEstado == 'P' || r.RevEstado == 'A'));
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
        try
        {
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
        catch
        {
            // Fallback defensivo para entornos con filas legacy/inconsistentes
            // que pueden romper el mapeo fuerte de EF (Guid/char(1)/DateTime).
            return await ListarAdminFallbackSqlAsync(page, limit, estado, ct);
        }
    }

    private async Task<(IReadOnlyList<ReservaAdminRowDto> Items, int Total)> ListarAdminFallbackSqlAsync(
        int page,
        int limit,
        char? estado,
        CancellationToken ct)
    {
        var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        var whereEstado = estado.HasValue
            ? " WHERE LEFT(COALESCE(rev_estado::text, ''), 1) = @estado"
            : string.Empty;

        await using var countCmd = conn.CreateCommand();
        countCmd.CommandText = $"SELECT COUNT(*) FROM ventas.reservas{whereEstado};";
        if (estado.HasValue)
        {
            var p = countCmd.CreateParameter();
            p.ParameterName = "@estado";
            p.Value = estado.Value.ToString();
            countCmd.Parameters.Add(p);
        }

        var countObj = await countCmd.ExecuteScalarAsync(ct);
        var total = 0;
        if (countObj is not null && int.TryParse(countObj.ToString(), out var parsedTotal))
            total = parsedTotal;

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
SELECT
  rev_guid::text,
  rev_codigo,
  COALESCE(cli_guid::text, ''),
  LEFT(COALESCE(rev_estado::text, 'P'), 1),
  COALESCE(rev_total, 0)::text,
  COALESCE(rev_fecha_reserva_utc::text, ''),
  COALESCE(atraccion_nombre_snap, ''),
  COALESCE(hor_fecha_snap, ''),
  COALESCE(hor_hora_inicio_snap, '')
FROM ventas.reservas
{whereEstado}
ORDER BY rev_fecha_reserva_utc DESC NULLS LAST
OFFSET @offset
LIMIT @limit;";

        var pOffset = cmd.CreateParameter();
        pOffset.ParameterName = "@offset";
        pOffset.Value = (page - 1) * limit;
        cmd.Parameters.Add(pOffset);

        var pLimit = cmd.CreateParameter();
        pLimit.ParameterName = "@limit";
        pLimit.Value = limit;
        cmd.Parameters.Add(pLimit);

        if (estado.HasValue)
        {
            var pEstado = cmd.CreateParameter();
            pEstado.ParameterName = "@estado";
            pEstado.Value = estado.Value.ToString();
            cmd.Parameters.Add(pEstado);
        }

        var items = new List<ReservaAdminRowDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var revGuidTxt = reader.GetString(0);
            if (!Guid.TryParse(revGuidTxt, out var revGuid))
                continue;

            var cliGuidTxt = reader.GetString(2);
            var cliGuid = Guid.TryParse(cliGuidTxt, out var cg) ? cg : Guid.Empty;

            var estadoTxt = reader.GetString(3);
            var estadoChar = string.IsNullOrWhiteSpace(estadoTxt) ? 'P' : char.ToUpperInvariant(estadoTxt[0]);

            var totalTxt = reader.GetString(4);
            var totalDec = decimal.TryParse(totalTxt, out var td) ? td : 0m;

            var fechaTxt = reader.GetString(5);
            var fecha = DateTime.TryParse(fechaTxt, out var dt) ? dt : DateTime.UtcNow;

            items.Add(new ReservaAdminRowDto
            {
                RevGuid = revGuid,
                RevCodigo = reader.GetString(1),
                CliGuid = cliGuid,
                Estado = estadoChar,
                Total = totalDec,
                FechaReserva = fecha,
                AtraccionNombreSnap = reader.GetString(6),
                HorFechaSnap = reader.GetString(7),
                HorHoraInicioSnap = reader.GetString(8),
            });
        }

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

    public async Task<bool> ActualizarEstadoAsync(
        Guid revGuid,
        char nuevoEstado,
        string motivo,
        string usuario,
        string ip,
        CancellationToken ct = default)
    {
        var entity = await _db.Reservas.FirstOrDefaultAsync(r => r.RevGuid == revGuid, ct);
        if (entity is null)
            return false;

        entity.RevEstado = nuevoEstado;
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
