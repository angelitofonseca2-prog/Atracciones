using Microservicio.Atracciones.DataAccess.Entities.Reservas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class HorarioConfiguration : IEntityTypeConfiguration<HorarioEntity>
    {
        public void Configure(EntityTypeBuilder<HorarioEntity> builder)
        {
            builder.ToTable("horario", "atracciones");
            builder.HasKey(x => x.HorId);

            builder.Property(x => x.HorId).HasColumnName("hor_id").UseIdentityAlwaysColumn();
            builder.Property(x => x.HorGuid).HasColumnName("hor_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
            builder.Property(x => x.TckId).HasColumnName("tck_id").IsRequired();
            builder.Property(x => x.HorFecha).HasColumnName("hor_fecha").HasColumnType("date").IsRequired();
            builder.Property(x => x.HorHoraInicio).HasColumnName("hor_hora_inicio").HasColumnType("time").IsRequired();
            builder.Property(x => x.HorHoraFin).HasColumnName("hor_hora_fin").HasColumnType("time");
            builder.Property(x => x.HorCuposDisponibles).HasColumnName("hor_cupos_disponibles").IsRequired();

            builder.Property(x => x.HorFechaIngreso).HasColumnName("hor_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'").IsRequired();
            builder.Property(x => x.HorUsuarioIngreso).HasColumnName("hor_usuario_ingreso").HasMaxLength(100).IsRequired();
            builder.Property(x => x.HorIpIngreso).HasColumnName("hor_ip_ingreso").HasMaxLength(45).IsRequired();

            builder.Property(x => x.HorFechaMod).HasColumnName("hor_fecha_mod");
            builder.Property(x => x.HorUsuarioMod).HasColumnName("hor_usuario_mod").HasMaxLength(100);
            builder.Property(x => x.HorIpMod).HasColumnName("hor_ip_mod").HasMaxLength(45);

            builder.Property(x => x.HorFechaEliminacion).HasColumnName("hor_fecha_eliminacion");
            builder.Property(x => x.HorUsuarioEliminacion).HasColumnName("hor_usuario_eliminacion").HasMaxLength(100);
            builder.Property(x => x.HorIpEliminacion).HasColumnName("hor_ip_eliminacion").HasMaxLength(45);

            builder.Property(x => x.HorEstado).HasColumnName("hor_estado").HasColumnType("char(1)").HasDefaultValue('A').IsRequired();

            builder.HasIndex(x => x.HorGuid).IsUnique().HasDatabaseName("uk_horario_guid");
            // Slot único: un ticket no puede tener dos turnos iguales el mismo día a la misma hora
            builder.HasIndex(x => new { x.TckId, x.HorFecha, x.HorHoraInicio })
                .IsUnique()
                .HasDatabaseName("uk_horario_slot");

            builder.HasOne(x => x.Ticket)
                .WithMany(t => t.Horarios)
                .HasForeignKey(x => x.TckId)
                .HasConstraintName("fk_horario_ticket");
        }
    }
}
