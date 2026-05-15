using Atracciones.MsOrquestador.DataAccess.Context;
using Atracciones.MsOrquestador.DataAccess.Entities;
using Atracciones.MsOrquestador.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsOrquestador.DataAccess.Repositories;

public sealed class SagaRepository : ISagaRepository
{
    private readonly OrquestadorDbContext _db;

    public SagaRepository(OrquestadorDbContext db) => _db = db;

    public async Task<Guid> IniciarSagaAsync(string tipo, string correlationId, CancellationToken ct = default)
    {
        var id = Guid.NewGuid();
        _db.SagaStates.Add(new SagaStateEntity
        {
            SagaId = id,
            Tipo = tipo,
            Estado = "EN_PROGRESO",
            CorrelationId = correlationId,
            InicioUtc = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync(ct);
        return id;
    }

    public async Task RegistrarPasoAsync(
        Guid sagaId,
        string paso,
        string estado,
        string? requestPayload,
        string? responsePayload,
        string? error,
        CancellationToken ct = default)
    {
        _db.SagaPasos.Add(new SagaPasoEntity
        {
            SagaId = sagaId,
            Paso = paso,
            Estado = estado,
            RequestPayload = requestPayload,
            ResponsePayload = responsePayload,
            Error = error,
            FechaUtc = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task CompletarSagaAsync(Guid sagaId, string estadoFinal, CancellationToken ct = default)
    {
        await _db.SagaStates.Where(s => s.SagaId == sagaId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Estado, estadoFinal)
                .SetProperty(x => x.FinUtc, DateTime.UtcNow), ct);
    }
}
