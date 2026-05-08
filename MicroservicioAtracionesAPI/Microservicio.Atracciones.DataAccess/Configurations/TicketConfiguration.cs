using Microservicio.Atracciones.DataAccess.Entities.Reservas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class TicketConfiguration : IEntityTypeConfiguration<TicketEntity>
    {
        public void Configure(EntityTypeBuilder<TicketEntity> builder)
        {
            builder.ToTable("ticket", "atracciones");
            builder.HasKey(x => x.TckId);

            builder.Property(x => x.TckId).HasColumnName("tck_id").UseIdentityAlwaysColumn();
            builder.Property(x => x.TckGuid).HasColumnName("tck_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
            builder.Property(x => x.AtId).HasColumnName("at_id").IsRequired();
            builder.Property(x => x.TckTitulo).HasColumnName("tck_titulo").HasMaxLength(150).IsRequired();
            builder.Property(x => x.TckPrecio).HasColumnName("tck_precio").HasColumnType("numeric(10,2)").IsRequired();
            builder.Property(x => x.TckTipoParticipante).HasColumnName("tck_tipo_participante").HasMaxLength(30).HasDefaultValue("Adulto").IsRequired();
            builder.Property(x => x.TckCapacidadMaxima).HasColumnName("tck_capacidad_maxima").IsRequired();
            builder.Property(x => x.TckCuposDisponibles).HasColumnName("tck_cupos_disponibles").IsRequired();

            builder.Property(x => x.TckFechaIngreso).HasColumnName("tck_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'").IsRequired();
            builder.Property(x => x.TckUsuarioIngreso).HasColumnName("tck_usuario_ingreso").HasMaxLength(100).IsRequired();
            builder.Property(x => x.TckIpIngreso).HasColumnName("tck_ip_ingreso").HasMaxLength(45).IsRequired();

            builder.Property(x => x.TckFechaMod).HasColumnName("tck_fecha_mod");
            builder.Property(x => x.TckUsuarioMod).HasColumnName("tck_usuario_mod").HasMaxLength(100);
            builder.Property(x => x.TckIpMod).HasColumnName("tck_ip_mod").HasMaxLength(45);

            builder.Property(x => x.TckFechaEliminacion).HasColumnName("tck_fecha_eliminacion");
            builder.Property(x => x.TckUsuarioEliminacion).HasColumnName("tck_usuario_eliminacion").HasMaxLength(100);
            builder.Property(x => x.TckIpEliminacion).HasColumnName("tck_ip_eliminacion").HasMaxLength(45);

            builder.Property(x => x.TckEstado).HasColumnName("tck_estado").HasColumnType("char(1)").HasDefaultValue('A').IsRequired();

            builder.HasIndex(x => x.TckGuid).IsUnique().HasDatabaseName("uk_ticket_guid");

            builder.HasOne(x => x.Atraccion)
                .WithMany(a => a.Tickets)
                .HasForeignKey(x => x.AtId)
                .HasConstraintName("fk_ticket_atraccion");
        }
    }
}
