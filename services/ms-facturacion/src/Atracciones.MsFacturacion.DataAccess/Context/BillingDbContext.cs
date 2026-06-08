using Atracciones.MsFacturacion.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsFacturacion.DataAccess.Context;

public sealed class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options)
        : base(options)
    {
    }

    public DbSet<FacturaEntity> Facturas => Set<FacturaEntity>();
    public DbSet<DatosFacturacionEntity> DatosFacturacion => Set<DatosFacturacionEntity>();
    public DbSet<ProcessedEventEntity> ProcessedEvents => Set<ProcessedEventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("billing");

        modelBuilder.Entity<FacturaEntity>(b =>
        {
            b.ToTable("facturas");
            b.HasKey(x => x.FacGuid);
            b.Property(x => x.FacGuid).HasColumnName("fac_guid");
            b.Property(x => x.RevGuid).HasColumnName("rev_guid");
            b.Property(x => x.CliGuid).HasColumnName("cli_guid");
            b.Property(x => x.FacNumero).HasColumnName("fac_numero").HasMaxLength(48).IsRequired();
            b.Property(x => x.FacTotal).HasColumnName("fac_total").HasColumnType("numeric(12,2)");
            b.Property(x => x.FacMoneda).HasColumnName("fac_moneda").HasMaxLength(8).HasDefaultValue("USD");
            b.Property(x => x.FacFechaEmisionUtc).HasColumnName("fac_fecha_emision_utc");
            b.Property(x => x.FacEstado).HasColumnName("fac_estado").HasMaxLength(1).IsFixedLength().HasColumnType("char(1)");
            b.Property(x => x.RevCodigoSnap).HasColumnName("rev_codigo_snap").HasMaxLength(32).IsRequired();
            b.Property(x => x.FacUsuarioIngreso).HasColumnName("fac_usuario_ingreso").HasMaxLength(100).IsRequired();
            b.Property(x => x.FacIpIngreso).HasColumnName("fac_ip_ingreso").HasMaxLength(45).IsRequired();
            b.HasIndex(x => x.RevGuid).IsUnique();
            b.HasIndex(x => x.CliGuid);
            b.HasIndex(x => x.FacNumero).IsUnique();
        });

        modelBuilder.Entity<DatosFacturacionEntity>(b =>
        {
            b.ToTable("datos_facturacion");
            b.HasKey(x => x.DfacGuid);
            b.Property(x => x.DfacGuid).HasColumnName("dfac_guid");
            b.Property(x => x.FacGuid).HasColumnName("fac_guid");
            b.Property(x => x.DfacNombre).HasColumnName("dfac_nombre").HasMaxLength(300).IsRequired();
            b.Property(x => x.DfacCorreo).HasColumnName("dfac_correo").HasMaxLength(256).IsRequired();
            b.Property(x => x.DfacTelefono).HasColumnName("dfac_telefono").HasMaxLength(32);
            b.HasOne(x => x.Factura)
                .WithOne(x => x.Datos)
                .HasForeignKey<DatosFacturacionEntity>(x => x.FacGuid)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => x.FacGuid).IsUnique();
        });

        modelBuilder.Entity<ProcessedEventEntity>(b =>
        {
            b.ToTable("eventos_procesados");
            b.HasKey(x => x.EventId);
            b.Property(x => x.EventId).HasColumnName("event_id");
            b.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(120).IsRequired();
            b.Property(x => x.ProcessedUtc).HasColumnName("processed_utc");
        });
    }
}
