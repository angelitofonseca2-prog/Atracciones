using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class ImagenConfiguration : IEntityTypeConfiguration<ImagenEntity>
    {
        public void Configure(EntityTypeBuilder<ImagenEntity> builder)
        {
            builder.ToTable("imagen", "atracciones");
            builder.HasKey(x => x.ImgId);

            builder.Property(x => x.ImgId).HasColumnName("img_id").UseIdentityAlwaysColumn();
            builder.Property(x => x.ImgGuid).HasColumnName("img_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
            builder.Property(x => x.ImgUrl).HasColumnName("img_url").HasMaxLength(500).IsRequired();
            builder.Property(x => x.ImgDescripcion).HasColumnName("img_descripcion").HasMaxLength(200);

            builder.Property(x => x.ImgFechaIngreso).HasColumnName("img_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'").IsRequired();
            builder.Property(x => x.ImgUsuarioIngreso).HasColumnName("img_usuario_ingreso").HasMaxLength(100).IsRequired();
            builder.Property(x => x.ImgIpIngreso).HasColumnName("img_ip_ingreso").HasMaxLength(45).IsRequired();

            builder.Property(x => x.ImgFechaMod).HasColumnName("img_fecha_mod");
            builder.Property(x => x.ImgUsuarioMod).HasColumnName("img_usuario_mod").HasMaxLength(100);
            builder.Property(x => x.ImgIpMod).HasColumnName("img_ip_mod").HasMaxLength(45);

            builder.Property(x => x.ImgFechaEliminacion).HasColumnName("img_fecha_eliminacion");
            builder.Property(x => x.ImgUsuarioEliminacion).HasColumnName("img_usuario_eliminacion").HasMaxLength(100);
            builder.Property(x => x.ImgIpEliminacion).HasColumnName("img_ip_eliminacion").HasMaxLength(45);

            builder.Property(x => x.ImgEstado).HasColumnName("img_estado").HasColumnType("char(1)").HasDefaultValue('A').IsRequired();

            builder.HasIndex(x => x.ImgGuid).IsUnique().HasDatabaseName("uk_imagen_guid");
        }
    }
}
