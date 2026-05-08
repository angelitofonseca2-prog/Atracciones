using Microservicio.Atracciones.DataAccess.Entities.Reservas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class ReseniaConfiguration : IEntityTypeConfiguration<ReseniaEntity>
    {
        public void Configure(EntityTypeBuilder<ReseniaEntity> builder)
        {
            builder.ToTable("resenia", "atracciones");
            builder.HasKey(x => x.RsnId);

            builder.Property(x => x.RsnId).HasColumnName("rsn_id").UseIdentityAlwaysColumn();
            builder.Property(x => x.RsnGuid).HasColumnName("rsn_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
            builder.Property(x => x.AtId).HasColumnName("at_id").IsRequired();
            builder.Property(x => x.RevId).HasColumnName("rev_id").IsRequired();
            builder.Property(x => x.RsnComentario).HasColumnName("rsn_comentario").HasMaxLength(1000);
            builder.Property(x => x.RsnRating).HasColumnName("rsn_rating").IsRequired();

            builder.Property(x => x.RsnFechaCreacion).HasColumnName("rsn_fecha_creacion").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'").IsRequired();
            builder.Property(x => x.RsnUsuarioCreacion).HasColumnName("rsn_usuario_creacion").HasMaxLength(100).IsRequired();
            builder.Property(x => x.RsnIpCreacion).HasColumnName("rsn_ip_creacion").HasMaxLength(45).IsRequired();

            builder.Property(x => x.RsnFechaMod).HasColumnName("rsn_fecha_mod");
            builder.Property(x => x.RsnUsuarioMod).HasColumnName("rsn_usuario_mod").HasMaxLength(100);
            builder.Property(x => x.RsnIpMod).HasColumnName("rsn_ip_mod").HasMaxLength(45);

            builder.Property(x => x.RsnFechaEliminacion).HasColumnName("rsn_fecha_eliminacion");
            builder.Property(x => x.RsnUsuarioEliminacion).HasColumnName("rsn_usuario_eliminacion").HasMaxLength(100);
            builder.Property(x => x.RsnIpEliminacion).HasColumnName("rsn_ip_eliminacion").HasMaxLength(45);

            builder.Property(x => x.RsnEstado).HasColumnName("rsn_estado").HasColumnType("char(1)").HasDefaultValue('A').IsRequired();

            builder.HasIndex(x => x.RsnGuid).IsUnique().HasDatabaseName("uk_resenia_guid");
            // 1 reseña por reserva
            builder.HasIndex(x => x.RevId).IsUnique().HasDatabaseName("uk_resenia_reserva");

            builder.HasOne(x => x.Atraccion)
                .WithMany(a => a.Resenias)
                .HasForeignKey(x => x.AtId)
                .HasConstraintName("fk_resenia_atraccion");

            // FK diferida en DDL (ciclo Resenia → Reserva → Resenia)
            builder.HasOne(x => x.Reserva)
                .WithOne(r => r.Resenia)
                .HasForeignKey<ReseniaEntity>(x => x.RevId)
                .HasConstraintName("fk_resenia_reserva");
        }
    }
}
