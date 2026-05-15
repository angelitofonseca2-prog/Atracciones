using Atracciones.MsReservas.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsReservas.DataAccess.Context;

public sealed class VentasDbContext : DbContext
{
    public VentasDbContext(DbContextOptions<VentasDbContext> options)
        : base(options)
    {
    }

    public DbSet<ReservaEntity> Reservas => Set<ReservaEntity>();
    public DbSet<ReservaDetalleEntity> ReservaDetalles => Set<ReservaDetalleEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ventas");

        modelBuilder.Entity<ReservaEntity>(b =>
        {
            b.ToTable("reservas");
            b.HasKey(x => x.RevGuid);
            b.Property(x => x.RevGuid).HasColumnName("rev_guid");
            b.Property(x => x.CliGuid).HasColumnName("cli_guid");
            b.Property(x => x.AtGuid).HasColumnName("at_guid");
            b.Property(x => x.HorGuid).HasColumnName("hor_guid");
            b.Property(x => x.RevCodigo).HasColumnName("rev_codigo").HasMaxLength(32).IsRequired();
            b.Property(x => x.RevEstado).HasColumnName("rev_estado").HasMaxLength(1).IsFixedLength().HasColumnType("char(1)");
            b.Property(x => x.RevSubtotal).HasColumnName("rev_subtotal").HasColumnType("numeric(12,2)");
            b.Property(x => x.RevValorIva).HasColumnName("rev_valor_iva").HasColumnType("numeric(12,2)");
            b.Property(x => x.RevTotal).HasColumnName("rev_total").HasColumnType("numeric(12,2)");
            b.Property(x => x.RevMoneda).HasColumnName("rev_moneda").HasMaxLength(8).HasDefaultValue("USD");
            b.Property(x => x.RevOrigenCanal).HasColumnName("rev_origen_canal").HasMaxLength(50);
            b.Property(x => x.RevFechaReservaUtc).HasColumnName("rev_fecha_reserva_utc");
            b.Property(x => x.RevUsuarioIngreso).HasColumnName("rev_usuario_ingreso").HasMaxLength(100).IsRequired();
            b.Property(x => x.RevIpIngreso).HasColumnName("rev_ip_ingreso").HasMaxLength(45).IsRequired();
            b.Property(x => x.AtraccionNombreSnap).HasColumnName("atraccion_nombre_snap").HasMaxLength(200).IsRequired();
            b.Property(x => x.HorFechaSnap).HasColumnName("hor_fecha_snap").HasMaxLength(16).IsRequired();
            b.Property(x => x.HorHoraInicioSnap).HasColumnName("hor_hora_inicio_snap").HasMaxLength(16).IsRequired();
            b.Property(x => x.HorHoraFinSnap).HasColumnName("hor_hora_fin_snap").HasMaxLength(16).IsRequired();
            b.HasIndex(x => x.CliGuid);
            b.HasIndex(x => x.RevCodigo).IsUnique();
        });

        modelBuilder.Entity<ReservaDetalleEntity>(b =>
        {
            b.ToTable("reserva_detalle");
            b.HasKey(x => x.RdetGuid);
            b.Property(x => x.RdetGuid).HasColumnName("rdet_guid");
            b.Property(x => x.RevGuid).HasColumnName("rev_guid");
            b.Property(x => x.TckGuid).HasColumnName("tck_guid");
            b.Property(x => x.Cantidad).HasColumnName("cantidad");
            b.Property(x => x.PrecioUnit).HasColumnName("precio_unit").HasColumnType("numeric(12,2)");
            b.Property(x => x.SubtotalLinea).HasColumnName("subtotal_linea").HasColumnType("numeric(12,2)");
            b.Property(x => x.TipoParticipante).HasColumnName("tipo_participante").HasMaxLength(50).IsRequired();
            b.HasOne(x => x.Reserva)
                .WithMany(x => x.Detalle)
                .HasForeignKey(x => x.RevGuid)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
