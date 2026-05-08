using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microservicio.Atracciones.DataAccess.Entities.Seguridad;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class RolConfiguration : IEntityTypeConfiguration<RolEntity>
    {
        public void Configure(EntityTypeBuilder<RolEntity> builder)
        {
            builder.ToTable("roles", "atracciones");
            builder.HasKey(x => x.RolId);

            builder.Property(x => x.RolId)
                .HasColumnName("rol_id")
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.RolGuid)
                .HasColumnName("rol_guid")
                .HasDefaultValueSql("gen_random_uuid()")
                .IsRequired();

            builder.Property(x => x.RolDescripcion)
                .HasColumnName("rol_descripcion")
                .HasMaxLength(80)
                .IsRequired();

            builder.Property(x => x.RolFechaIngreso)
                .HasColumnName("rol_fecha_ingreso")
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'")
                .IsRequired();

            builder.Property(x => x.RolUsuarioIngreso)
                .HasColumnName("rol_usuario_ingreso")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.RolIpIngreso)
                .HasColumnName("rol_ip_ingreso")
                .HasMaxLength(45)
                .IsRequired();

            builder.Property(x => x.RolFechaEliminacion)
                .HasColumnName("rol_fecha_eliminacion");

            builder.Property(x => x.RolUsuarioEliminacion)
                .HasColumnName("rol_usuario_eliminacion")
                .HasMaxLength(100);

            builder.Property(x => x.RolIpEliminacion)
                .HasColumnName("rol_ip_eliminacion")
                .HasMaxLength(45);

            builder.Property(x => x.RolEstado)
                .HasColumnName("rol_estado")
                .HasColumnType("char(1)")
                .HasDefaultValue('A')
                .IsRequired();

            builder.HasIndex(x => x.RolGuid)
                .IsUnique()
                .HasDatabaseName("uk_roles_guid");
        }
    }
}
