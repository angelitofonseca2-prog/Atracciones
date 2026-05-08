using Microsoft.EntityFrameworkCore;
using Microservicio.Atracciones.DataAccess.Entities.Seguridad;
using Microservicio.Atracciones.DataAccess.Entities.Clientes;
using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using Microservicio.Atracciones.DataAccess.Entities.Reservas;
using Microservicio.Atracciones.DataAccess.Entities.Facturacion;
using Microservicio.Atracciones.DataAccess.Entities.Auditoria;
using Microservicio.Atracciones.DataAccess.Configurations;

namespace Microservicio.Atracciones.DataAccess.Context;

/// <summary>
/// Contexto principal de EF Core para el microservicio de Atracciones.
/// Conecta con PostgreSQL (esquema: atracciones) y expone los 22 DbSet
/// que representan las tablas del modelo de datos v3.
/// </summary>
public class AtraccionesDbContext : DbContext
{
    public AtraccionesDbContext(DbContextOptions<AtraccionesDbContext> options)
        : base(options)
    {
    }

    // ----------------------------------------------------------------
    //  SECCIÓN 1: Seguridad y acceso
    // ----------------------------------------------------------------
    public DbSet<RolEntity> Roles { get; set; }
    public DbSet<UsuarioEntity> Usuarios { get; set; }
    public DbSet<UsuarioRolEntity> UsuariosRoles { get; set; }

    // ----------------------------------------------------------------
    //  SECCIÓN 2: Clientes
    // ----------------------------------------------------------------
    public DbSet<ClienteEntity> Clientes { get; set; }

    // ----------------------------------------------------------------
    //  SECCIÓN 3: Catálogos base
    // ----------------------------------------------------------------
    public DbSet<DestinoEntity> Destinos { get; set; }
    public DbSet<CategoriaEntity> Categorias { get; set; }
    public DbSet<IdiomaEntity> Idiomas { get; set; }
    public DbSet<IncluyeEntity> Incluyes { get; set; }
    public DbSet<ImagenEntity> Imagenes { get; set; }

    // ----------------------------------------------------------------
    //  SECCIÓN 4: Atracción y tablas pivote N:M
    // ----------------------------------------------------------------
    public DbSet<AtraccionEntity> Atracciones { get; set; }
    public DbSet<CategoriaAtraccionEntity> CategoriasAtracciones { get; set; }
    public DbSet<IdiomaAtraccionEntity> IdiomasAtracciones { get; set; }
    public DbSet<ImagenAtraccionEntity> ImagenesAtracciones { get; set; }
    public DbSet<AtraccionIncluyeEntity> AtraccionesIncluyen { get; set; }

    // ----------------------------------------------------------------
    //  SECCIÓN 5: Disponibilidad
    // ----------------------------------------------------------------
    public DbSet<TicketEntity> Tickets { get; set; }
    public DbSet<HorarioEntity> Horarios { get; set; }

    // ----------------------------------------------------------------
    //  SECCIÓN 6: Reseñas
    // ----------------------------------------------------------------
    public DbSet<ReseniaEntity> Resenias { get; set; }

    // ----------------------------------------------------------------
    //  SECCIÓN 7: Reservas y facturación
    // ----------------------------------------------------------------
    public DbSet<ReservaEntity> Reservas { get; set; }
    public DbSet<ReservaDetalleEntity> ReservasDetalle { get; set; }
    public DbSet<FacturaEntity> Facturas { get; set; }
    public DbSet<DatosFacturacionEntity> DatosFacturacion { get; set; }

    // ----------------------------------------------------------------
    //  SECCIÓN 8: Auditoría
    // ----------------------------------------------------------------
    public DbSet<AuditoriaLogEntity> AuditoriaLogs { get; set; }

    // ----------------------------------------------------------------
    //  Configuración del modelo
    // ----------------------------------------------------------------
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Establece el esquema por defecto para todas las tablas
        modelBuilder.HasDefaultSchema("atracciones");

        // Seguridad
        modelBuilder.ApplyConfiguration(new RolConfiguration());
        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new UsuarioRolConfiguration());

        // Clientes
        modelBuilder.ApplyConfiguration(new ClienteConfiguration());

        // Catálogos
        modelBuilder.ApplyConfiguration(new DestinoConfiguration());
        modelBuilder.ApplyConfiguration(new CategoriaConfiguration());
        modelBuilder.ApplyConfiguration(new IdiomaConfiguration());
        modelBuilder.ApplyConfiguration(new IncluyeConfiguration());
        modelBuilder.ApplyConfiguration(new ImagenConfiguration());

        // Atracción y pivotes
        modelBuilder.ApplyConfiguration(new AtraccionConfiguration());
        modelBuilder.ApplyConfiguration(new CategoriaAtraccionConfiguration());
        modelBuilder.ApplyConfiguration(new IdiomaAtraccionConfiguration());
        modelBuilder.ApplyConfiguration(new ImagenAtraccionConfiguration());
        modelBuilder.ApplyConfiguration(new AtraccionIncluyeConfiguration());

        // Disponibilidad
        modelBuilder.ApplyConfiguration(new TicketConfiguration());
        modelBuilder.ApplyConfiguration(new HorarioConfiguration());

        // Reseñas
        modelBuilder.ApplyConfiguration(new ReseniaConfiguration());

        // Reservas y facturación
        modelBuilder.ApplyConfiguration(new ReservaConfiguration());
        modelBuilder.ApplyConfiguration(new ReservaDetalleConfiguration());
        modelBuilder.ApplyConfiguration(new FacturaConfiguration());
        modelBuilder.ApplyConfiguration(new DatosFacturacionConfiguration());

        // Auditoría
        modelBuilder.ApplyConfiguration(new AuditoriaLogConfiguration());

        // Forzar minúsculas en todos los nombres de tabla y columna.
        // PostgreSQL convierte identificadores sin comillas dobles a minúsculas,
        // por lo que los nombres generados por EF Core deben coincidir exactamente.
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.GetTableName()!.ToLower());
            entity.SetSchema("atracciones");

            foreach (var prop in entity.GetProperties())
                prop.SetColumnName(prop.GetColumnName().ToLower());
        }

        base.OnModelCreating(modelBuilder);
    }
}