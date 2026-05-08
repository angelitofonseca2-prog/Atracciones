using Microservicio.Atracciones.DataAccess.Entities.Reservas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class ReservaConfiguration : IEntityTypeConfiguration<ReservaEntity>
    {
        public void Configure(EntityTypeBuilder<ReservaEntity> builder)
        {
            builder.ToTable("reservas", "atracciones");
            builder.HasKey(x => x.RevId);

            builder.Property(x => x.RevId).HasColumnName("rev_id").UseIdentityAlwaysColumn();
            builder.Property(x => x.RevGuid).HasColumnName("rev_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
            builder.Property(x => x.RevCodigo).HasColumnName("rev_codigo").HasMaxLength(20).IsRequired();
            builder.Property(x => x.CliId).HasColumnName("cli_id").IsRequired();
            builder.Property(x => x.HorId).HasColumnName("hor_id").IsRequired();

            builder.Property(x => x.RevFechaReservaUtc)
                .HasColumnName("rev_fecha_reserva_utc")
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'")
                .IsRequired();

            builder.Property(x => x.RevSubtotal).HasColumnName("rev_subtotal").HasColumnType("numeric(10,2)").IsRequired();
            builder.Property(x => x.RevValorIva).HasColumnName("rev_valor_iva").HasColumnType("numeric(10,2)").IsRequired();
            builder.Property(x => x.RevTotal).HasColumnName("rev_total").HasColumnType("numeric(10,2)").IsRequired();
            builder.Property(x => x.RevOrigenCanal).HasColumnName("rev_origen_canal").HasMaxLength(50);

            builder.Property(x => x.RevUsuarioIngreso).HasColumnName("rev_usuario_ingreso").HasMaxLength(100).IsRequired();
            builder.Property(x => x.RevIpIngreso).HasColumnName("rev_ip_ingreso").HasMaxLength(45).IsRequired();

            builder.Property(x => x.RevFechaMod).HasColumnName("rev_fecha_mod");
            builder.Property(x => x.RevUsuarioMod).HasColumnName("rev_usuario_mod").HasMaxLength(100);
            builder.Property(x => x.RevIpMod).HasColumnName("rev_ip_mod").HasMaxLength(45);

            builder.Property(x => x.RevFechaCancelacion).HasColumnName("rev_fecha_cancelacion");
            builder.Property(x => x.RevUsuarioCancelacion).HasColumnName("rev_usuario_cancelacion").HasMaxLength(100);
            builder.Property(x => x.RevIpCancelacion).HasColumnName("rev_ip_cancelacion").HasMaxLength(45);
            builder.Property(x => x.RevMotivoCancelacion).HasColumnName("rev_motivo_cancelacion").HasMaxLength(300);

            // Estado: 'A' = Activa, 'I' = Inactiva, 'C' = Cancelada
            builder.Property(x => x.RevEstado).HasColumnName("rev_estado").HasColumnType("char(1)").HasDefaultValue('A').IsRequired();

            builder.HasIndex(x => x.RevGuid).IsUnique().HasDatabaseName("uk_reservas_guid");
            builder.HasIndex(x => x.RevCodigo).IsUnique().HasDatabaseName("uk_reservas_codigo");

            builder.HasOne(x => x.Cliente)
                .WithMany(c => c.Reservas)
                .HasForeignKey(x => x.CliId)
                .HasConstraintName("fk_reservas_cliente");

            builder.HasOne(x => x.Horario)
                .WithMany(h => h.Reservas)
                .HasForeignKey(x => x.HorId)
                .HasConstraintName("fk_reservas_horario");
        }
    }
}
