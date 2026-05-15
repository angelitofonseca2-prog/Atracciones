using Atracciones.MsFacturacion.DataAccess.Context;
using Atracciones.MsFacturacion.DataAccess.Entities;
using Atracciones.MsFacturacion.DataManagement.Interfaces;
using Atracciones.MsFacturacion.DataManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsFacturacion.DataAccess.Repositories;

public sealed class FacturaRepository : IFacturaRepository
{
    private readonly BillingDbContext _db;

    public FacturaRepository(BillingDbContext db) => _db = db;

    public async Task<FacturaEmitidaDto> EmitirAsync(EmitirFacturaInternaDto dto, CancellationToken ct = default)
    {
        var existente = await _db.Facturas
            .AsNoTracking()
            .Include(f => f.Datos)
            .FirstOrDefaultAsync(f => f.RevGuid == dto.RevGuid, ct);
        if (existente is not null)
            return MapEmitida(existente);

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var dup = await _db.Facturas.AnyAsync(f => f.RevGuid == dto.RevGuid, ct);
            if (dup)
            {
                var again = await _db.Facturas.Include(f => f.Datos).FirstAsync(f => f.RevGuid == dto.RevGuid, ct);
                await tx.CommitAsync(ct);
                return MapEmitida(again);
            }

            var facGuid = Guid.NewGuid();
            var dfacGuid = Guid.NewGuid();
            var sufijo = facGuid.ToString("N")[..8].ToUpperInvariant();
            var numero = $"FAC-{DateTime.UtcNow:yyyy}-{sufijo}";

            var fac = new FacturaEntity
            {
                FacGuid = facGuid,
                RevGuid = dto.RevGuid,
                CliGuid = dto.CliGuid,
                FacNumero = numero,
                FacTotal = dto.Total,
                FacMoneda = string.IsNullOrWhiteSpace(dto.Moneda) ? "USD" : dto.Moneda.Trim(),
                FacFechaEmisionUtc = DateTime.UtcNow,
                FacEstado = 'A',
                RevCodigoSnap = dto.RevCodigoSnap.Trim(),
                FacUsuarioIngreso = string.IsNullOrWhiteSpace(dto.UsuarioEmision) ? "sistema" : dto.UsuarioEmision.Trim(),
                FacIpIngreso = string.IsNullOrWhiteSpace(dto.IpEmision) ? "0.0.0.0" : dto.IpEmision.Trim(),
                Datos = new DatosFacturacionEntity
                {
                    DfacGuid = dfacGuid,
                    FacGuid = facGuid,
                    DfacNombre = dto.NombreReceptor.Trim(),
                    DfacCorreo = dto.CorreoReceptor.Trim(),
                    DfacTelefono = string.IsNullOrWhiteSpace(dto.TelefonoReceptor) ? null : dto.TelefonoReceptor.Trim(),
                },
            };

            _db.Facturas.Add(fac);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            await _db.Entry(fac).Reference(f => f.Datos).LoadAsync(ct);
            return MapEmitida(fac);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<FacturaEmitidaDto?> ObtenerPorGuidAsync(Guid facGuid, CancellationToken ct = default)
    {
        var f = await _db.Facturas.Include(x => x.Datos).AsNoTracking().FirstOrDefaultAsync(x => x.FacGuid == facGuid, ct);
        return f is null ? null : MapEmitida(f);
    }

    public async Task<(IReadOnlyList<FacturaEmitidaDto> Items, int Total)> ListarPorClienteAsync(
        Guid cliGuid,
        int page,
        int limit,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var q = _db.Facturas.AsNoTracking().Include(f => f.Datos).Where(f => f.CliGuid == cliGuid).OrderByDescending(f => f.FacFechaEmisionUtc);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((page - 1) * limit).Take(limit).ToListAsync(ct);
        return (rows.Select(MapEmitida).ToList(), total);
    }

    public async Task<(IReadOnlyList<FacturaAdminRowDto> Items, int Total)> ListarAdminAsync(
        int page,
        int limit,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var q = _db.Facturas.AsNoTracking().OrderByDescending(f => f.FacFechaEmisionUtc);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((page - 1) * limit).Take(limit)
            .Select(f => new FacturaAdminRowDto
            {
                FacGuid = f.FacGuid,
                FacNumero = f.FacNumero,
                RevGuid = f.RevGuid,
                CliGuid = f.CliGuid,
                RevCodigoSnap = f.RevCodigoSnap,
                Total = f.FacTotal,
                Moneda = f.FacMoneda,
                FechaEmisionUtc = f.FacFechaEmisionUtc,
                Estado = f.FacEstado,
            })
            .ToListAsync(ct);

        return (rows, total);
    }

    private static FacturaEmitidaDto MapEmitida(FacturaEntity f)
    {
        var datos = f.Datos;
        return new FacturaEmitidaDto
        {
            FacGuid = f.FacGuid,
            FacNumero = f.FacNumero,
            RevGuid = f.RevGuid,
            CliGuid = f.CliGuid,
            Total = f.FacTotal,
            Moneda = f.FacMoneda,
            FechaEmisionUtc = f.FacFechaEmisionUtc,
            Estado = f.FacEstado,
            NombreReceptor = datos?.DfacNombre ?? string.Empty,
            CorreoReceptor = datos?.DfacCorreo ?? string.Empty,
            RevCodigoSnap = f.RevCodigoSnap,
        };
    }
}
