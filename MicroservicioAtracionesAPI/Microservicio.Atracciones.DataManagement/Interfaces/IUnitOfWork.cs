using Microservicio.Atracciones.DataAccess.Repositories.Interfaces;

namespace Microservicio.Atracciones.DataManagement.Interfaces;

// ====================================================================
//  UNIT OF WORK
// ====================================================================

/// <summary>
/// Define los repositorios disponibles y el punto único de guardado.
/// Business nunca llama a SaveChanges directamente; lo hace a través
/// de esta interfaz, garantizando que todas las operaciones de una
/// transacción se confirmen o reviertan juntas.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IUsuarioRepository Usuarios { get; }
    IClienteRepository Clientes { get; }
    IDestinoRepository Destinos { get; }
    IAtraccionRepository Atracciones { get; }
    ITicketRepository Tickets { get; }
    IReservaRepository Reservas { get; }
    IFacturaRepository Facturas { get; }
    IReseniaRepository Resenias { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();
}
