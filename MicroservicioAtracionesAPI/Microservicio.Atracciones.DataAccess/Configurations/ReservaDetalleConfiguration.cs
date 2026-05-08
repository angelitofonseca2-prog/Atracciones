using Microservicio.Atracciones.DataAccess.Entities.Reservas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class ReservaDetalleConfiguration : IEntityTypeConfiguration<ReservaDetalleEntity>
    {
        public void Configure(EntityTypeBuilder<ReservaDetalleEntity> builder)
        {
            builder.ToTable("reserva_detalle", "atracciones");
            builder.HasKey(x => x.RdetId);

            builder.Property(x => x.RdetId).HasColumnName("rdet_id").UseIdentityAlwaysColumn();
            builder.Property(x => x.RdetGuid).HasColumnName("rdet_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
            builder.Property(x => x.RevId).HasColumnName("rev_id").IsRequired();
            builder.Property(x => x.TckId).HasColumnName("tck_id").IsRequired();
            builder.Property(x => x.RdetCantidad).HasColumnName("rdet_cantidad").IsRequired();
            builder.Property(x => x.RdetPrecioUnit).HasColumnName("rdet_precio_unit").HasColumnType("numeric(10,2)").IsRequired();
            builder.Property(x => x.RdetSubtotal).HasColumnName("rdet_subtotal").HasColumnType("numeric(10,2)").IsRequired();

            builder.Property(x => x.RdetFechaIngreso).HasColumnName("rdet_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'").IsRequired();
            builder.Property(x => x.RdetUsuarioIngreso).HasColumnName("rdet_usuario_ingreso").HasMaxLength(100).IsRequired();
            builder.Property(x => x.RdetIpIngreso).HasColumnName("rdet_ip_ingreso").HasMaxLength(45).IsRequired();

            builder.Property(x => x.RdetFechaEliminacion).HasColumnName("rdet_fecha_eliminacion");
            builder.Property(x => x.RdetUsuarioEliminacion).HasColumnName("rdet_usuario_eliminacion").HasMaxLength(100);
            builder.Property(x => x.RdetIpEliminacion).HasColumnName("rdet_ip_eliminacion").HasMaxLength(45);

            builder.Property(x => x.RdetEstado).HasColumnName("rdet_estado").HasColumnType("char(1)").HasDefaultValue('A').IsRequired();

            builder.HasIndex(x => x.RdetGuid).IsUnique().HasDatabaseName("uk_rdet_guid");
            // Un tipo de ticket por línea dentro de la misma reserva
            builder.HasIndex(x => new { x.RevId, x.TckId }).IsUnique().HasDatabaseName("uk_rdet_rev_tck");

            builder.HasOne(x => x.Reserva)
                .WithMany(r => r.ReservasDetalle)
                .HasForeignKey(x => x.RevId)
                .HasConstraintName("fk_rdet_reserva");

            builder.HasOne(x => x.Ticket)
                .WithMany(t => t.ReservasDetalle)
                .HasForeignKey(x => x.TckId)
                .HasConstraintName("fk_rdet_ticket");
        }
    }
}
