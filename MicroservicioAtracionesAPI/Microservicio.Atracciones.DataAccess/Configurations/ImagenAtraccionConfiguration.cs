using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class ImagenAtraccionConfiguration : IEntityTypeConfiguration<ImagenAtraccionEntity>
    {
        public void Configure(EntityTypeBuilder<ImagenAtraccionEntity> builder)
        {
            builder.ToTable("imagen_atraccion", "atracciones");
            builder.HasKey(x => new { x.ImgId, x.AtId });

            builder.Property(x => x.ImgId).HasColumnName("img_id").IsRequired();
            builder.Property(x => x.AtId).HasColumnName("at_id").IsRequired();
            builder.Property(x => x.ImaFechaIngreso).HasColumnName("ima_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'").IsRequired();
            builder.Property(x => x.ImaUsuarioIngreso).HasColumnName("ima_usuario_ingreso").HasMaxLength(100).IsRequired();
            builder.Property(x => x.ImaFechaEliminacion).HasColumnName("ima_fecha_eliminacion");
            builder.Property(x => x.ImaUsuarioEliminacion).HasColumnName("ima_usuario_eliminacion").HasMaxLength(100);
            builder.Property(x => x.ImaEstado).HasColumnName("ima_estado").HasColumnType("char(1)").HasDefaultValue('A').IsRequired();

            builder.HasOne(x => x.Imagen)
                .WithMany(i => i.ImagenesAtracciones)
                .HasForeignKey(x => x.ImgId)
                .HasConstraintName("fk_ima_imagen");

            builder.HasOne(x => x.Atraccion)
                .WithMany(a => a.ImagenesAtracciones)
                .HasForeignKey(x => x.AtId)
                .HasConstraintName("fk_ima_atraccion");
        }
    }
}
