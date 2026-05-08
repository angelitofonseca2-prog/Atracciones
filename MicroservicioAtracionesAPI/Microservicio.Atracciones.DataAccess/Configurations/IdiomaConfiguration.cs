using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class IdiomaConfiguration : IEntityTypeConfiguration<IdiomaEntity>
    {
        public void Configure(EntityTypeBuilder<IdiomaEntity> builder)
        {
            builder.ToTable("idioma", "atracciones");
            builder.HasKey(x => x.IdId);

            builder.Property(x => x.IdId).HasColumnName("id_id").UseIdentityAlwaysColumn();
            builder.Property(x => x.IdGuid).HasColumnName("id_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
            builder.Property(x => x.IdDescripcion).HasColumnName("id_descripcion").HasMaxLength(80).IsRequired();

            builder.Property(x => x.IdFechaIngreso).HasColumnName("id_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'").IsRequired();
            builder.Property(x => x.IdUsuarioIngreso).HasColumnName("id_usuario_ingreso").HasMaxLength(100).IsRequired();
            builder.Property(x => x.IdIpIngreso).HasColumnName("id_ip_ingreso").HasMaxLength(45).IsRequired();

            builder.Property(x => x.IdFechaMod).HasColumnName("id_fecha_mod");
            builder.Property(x => x.IdUsuarioMod).HasColumnName("id_usuario_mod").HasMaxLength(100);
            builder.Property(x => x.IdIpMod).HasColumnName("id_ip_mod").HasMaxLength(45);

            builder.Property(x => x.IdFechaEliminacion).HasColumnName("id_fecha_eliminacion");
            builder.Property(x => x.IdUsuarioEliminacion).HasColumnName("id_usuario_eliminacion").HasMaxLength(100);
            builder.Property(x => x.IdIpEliminacion).HasColumnName("id_ip_eliminacion").HasMaxLength(45);

            builder.Property(x => x.IdEstado).HasColumnName("id_estado").HasColumnType("char(1)").HasDefaultValue('A').IsRequired();

            builder.HasIndex(x => x.IdGuid).IsUnique().HasDatabaseName("uk_idioma_guid");
            builder.HasIndex(x => x.IdDescripcion).IsUnique().HasDatabaseName("uk_idioma_descripcion");
        }
    }
}
