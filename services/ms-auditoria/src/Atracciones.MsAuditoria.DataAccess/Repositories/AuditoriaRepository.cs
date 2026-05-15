using Atracciones.MsAuditoria.DataAccess.Context;
using Atracciones.MsAuditoria.DataAccess.Entities;
using Atracciones.MsAuditoria.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsAuditoria.DataAccess.Repositories;

public sealed class AuditoriaRepository : IAuditoriaRepository
{
    private readonly AuditoriaDbContext _db;

    public AuditoriaRepository(AuditoriaDbContext db) => _db = db;

    public async Task RegistrarEventoAsync(string tipo, string correlationId, string payloadJson, CancellationToken ct = default)
    {
        var cid = string.IsNullOrWhiteSpace(correlationId) ? "-" : correlationId.Trim();
        if (cid.Length > 128)
            cid = cid[..128];

        var row = new EventoAuditoriaEntity
        {
            EvtGuid = Guid.NewGuid(),
            EvtTipo = tipo.Trim(),
            CorrelationId = cid,
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson,
            FechaUtc = DateTime.UtcNow,
        };
        _db.Eventos.Add(row);
        await _db.SaveChangesAsync(ct);
    }
}
