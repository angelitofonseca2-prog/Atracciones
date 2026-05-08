using Microservicio.Atracciones.DataAccess.Context;
using Microservicio.Atracciones.DataAccess.Repositories;
using Microservicio.Atracciones.DataAccess.Repositories.Interfaces;
using Microservicio.Atracciones.DataManagement.Interfaces;

namespace Microservicio.Atracciones.DataManagement.Services;

/// <summary>
/// Implementa IUnitOfWork coordinando todos los repositorios y
/// centralizando el punto único de SaveChanges.
/// Se registra como Scoped en DI para garantizar que todos los
/// repositorios compartan el mismo DbContext dentro de una request.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AtraccionesDbContext _context;

    // Repositorios instanciados de forma lazy para no crearlos si no se usan
    private IUsuarioRepository? _usuarios;
    private IClienteRepository? _clientes;
    private IDestinoRepository? _destinos;
    private IAtraccionRepository? _atracciones;
    private ITicketRepository? _tickets;
    private IReservaRepository? _reservas;
    private IFacturaRepository? _facturas;
    private IReseniaRepository? _resenias;

    public UnitOfWork(AtraccionesDbContext context)
        => _context = context;

    public IUsuarioRepository Usuarios => _usuarios ??= new UsuarioRepository(_context);
    public IClienteRepository Clientes => _clientes ??= new ClienteRepository(_context);
    public IDestinoRepository Destinos => _destinos ??= new DestinoRepository(_context);
    public IAtraccionRepository Atracciones => _atracciones ??= new AtraccionRepository(_context);
    public ITicketRepository Tickets => _tickets ??= new TicketRepository(_context);
    public IReservaRepository Reservas => _reservas ??= new ReservaRepository(_context);
    public IFacturaRepository Facturas => _facturas ??= new FacturaRepository(_context);
    public IReseniaRepository Resenias => _resenias ??= new ReseniaRepository(_context);

    /// <summary>
    /// Confirma todos los cambios pendientes en el DbContext.
    /// Se llama UNA SOLA VEZ al final de cada operación de negocio.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync()
        => await _context.Database.BeginTransactionAsync();

    public async ValueTask DisposeAsync()
        => await _context.DisposeAsync();
}
