using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class AtraccionIncluyeConfiguration : IEntityTypeConfiguration<AtraccionIncluyeEntity>
    {
        public void Configure(EntityTypeBuilder<AtraccionIncluyeEntity> builder)
        {
            builder.ToTable("atraccion_incluye", "atracciones");
            builder.HasKey(x => new { x.IncId, x.AtId });

            builder.Property(x => x.IncId).HasColumnName("inc_id").IsRequired();
            builder.Property(x => x.AtId).HasColumnName("at_id").IsRequired();
            builder.Property(x => x.AiFechaIngreso).HasColumnName("ai_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'").IsRequired();
            builder.Property(x => x.AiUsuarioIngreso).HasColumnName("ai_usuario_ingreso").HasMaxLength(100).IsRequired();
            builder.Property(x => x.AiFechaEliminacion).HasColumnName("ai_fecha_eliminacion");
            builder.Property(x => x.AiUsuarioEliminacion).HasColumnName("ai_usuario_eliminacion").HasMaxLength(100);
            builder.Property(x => x.AiEstado).HasColumnName("ai_estado").HasColumnType("char(1)").HasDefaultValue('A').IsRequired();

            builder.HasOne(x => x.Incluye)
                .WithMany(i => i.AtraccionesIncluyen)
                .HasForeignKey(x => x.IncId)
                .HasConstraintName("fk_ai_incluye");

            builder.HasOne(x => x.Atraccion)
                .WithMany(a => a.AtraccionesIncluyen)
                .HasForeignKey(x => x.AtId)
                .HasConstraintName("fk_ai_atraccion");
        }
    }

}
