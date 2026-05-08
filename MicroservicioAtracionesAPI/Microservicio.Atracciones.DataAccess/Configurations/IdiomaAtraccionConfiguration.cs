using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class IdiomaAtraccionConfiguration : IEntityTypeConfiguration<IdiomaAtraccionEntity>
    {
        public void Configure(EntityTypeBuilder<IdiomaAtraccionEntity> builder)
        {
            builder.ToTable("idioma_atraccion", "atracciones");
            builder.HasKey(x => new { x.IdId, x.AtId });

            builder.Property(x => x.IdId).HasColumnName("id_id").IsRequired();
            builder.Property(x => x.AtId).HasColumnName("at_id").IsRequired();
            builder.Property(x => x.IaFechaIngreso).HasColumnName("ia_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'").IsRequired();
            builder.Property(x => x.IaUsuarioIngreso).HasColumnName("ia_usuario_ingreso").HasMaxLength(100).IsRequired();
            builder.Property(x => x.IaFechaEliminacion).HasColumnName("ia_fecha_eliminacion");
            builder.Property(x => x.IaUsuarioEliminacion).HasColumnName("ia_usuario_eliminacion").HasMaxLength(100);
            builder.Property(x => x.IaEstado).HasColumnName("ia_estado").HasColumnType("char(1)").HasDefaultValue('A').IsRequired();

            builder.HasOne(x => x.Idioma)
                .WithMany(i => i.IdiomasAtracciones)
                .HasForeignKey(x => x.IdId)
                .HasConstraintName("fk_ia_idioma");

            builder.HasOne(x => x.Atraccion)
                .WithMany(a => a.IdiomasAtracciones)
                .HasForeignKey(x => x.AtId)
                .HasConstraintName("fk_ia_atraccion");
        }
    }
}
