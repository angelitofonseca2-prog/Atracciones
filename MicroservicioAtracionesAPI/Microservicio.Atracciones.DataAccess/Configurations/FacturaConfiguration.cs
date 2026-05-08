using Microservicio.Atracciones.DataAccess.Entities.Facturacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class FacturaConfiguration : IEntityTypeConfiguration<FacturaEntity>
    {
        public void Configure(EntityTypeBuilder<FacturaEntity> builder)
        {
            builder.ToTable("facturas", "atracciones");
            builder.HasKey(x => x.FacId);

            builder.Property(x => x.FacId).HasColumnName("fac_id").UseIdentityAlwaysColumn();
            builder.Property(x => x.FacGuid).HasColumnName("fac_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
            builder.Property(x => x.RevId).HasColumnName("rev_id").IsRequired();
            builder.Property(x => x.FacNumero).HasColumnName("fac_numero").HasMaxLength(20).IsRequired();
            builder.Property(x => x.FacFechaEmision).HasColumnName("fac_fecha_emision").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'").IsRequired();
            builder.Property(x => x.FacTotal).HasColumnName("fac_total").HasColumnType("numeric(10,2)").IsRequired();
            builder.Property(x => x.FacObservacion).HasColumnName("fac_observacion").HasMaxLength(500);
            builder.Property(x => x.FacOrigenCanal).HasColumnName("fac_origen_canal").HasMaxLength(50);

            builder.Property(x => x.FacUsuarioIngreso).HasColumnName("fac_usuario_ingreso").HasMaxLength(100).IsRequired();
            builder.Property(x => x.FacIpIngreso).HasColumnName("fac_ip_ingreso").HasMaxLength(45).IsRequired();

            builder.Property(x => x.FacFechaMod).HasColumnName("fac_fecha_mod");
            builder.Property(x => x.FacUsuarioMod).HasColumnName("fac_usuario_mod").HasMaxLength(100);
            builder.Property(x => x.FacIpMod).HasColumnName("fac_ip_mod").HasMaxLength(45);

            builder.Property(x => x.FacFechaEliminacion).HasColumnName("fac_fecha_eliminacion");
            builder.Property(x => x.FacUsuarioEliminacion).HasColumnName("fac_usuario_eliminacion").HasMaxLength(100);
            builder.Property(x => x.FacIpEliminacion).HasColumnName("fac_ip_eliminacion").HasMaxLength(45);

            builder.Property(x => x.FacEstado).HasColumnName("fac_estado").HasColumnType("char(1)").HasDefaultValue('A').IsRequired();
            builder.Property(x => x.FacMotivoInhabilitacion).HasColumnName("fac_motivo_inhabilitacion").HasMaxLength(300);

            builder.HasIndex(x => x.FacGuid).IsUnique().HasDatabaseName("uk_facturas_guid");
            builder.HasIndex(x => x.FacNumero).IsUnique().HasDatabaseName("uk_facturas_numero");
            // 1 factura por reserva
            builder.HasIndex(x => x.RevId).IsUnique().HasDatabaseName("uk_facturas_reserva");

            builder.HasOne(x => x.Reserva)
                .WithOne(r => r.Factura)
                .HasForeignKey<FacturaEntity>(x => x.RevId)
                .HasConstraintName("fk_facturas_reserva");
        }
    }
}
