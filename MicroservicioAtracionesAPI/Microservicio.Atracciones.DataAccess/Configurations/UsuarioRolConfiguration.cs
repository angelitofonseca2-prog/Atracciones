using Microservicio.Atracciones.DataAccess.Entities.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRolEntity>
    {
        public void Configure(EntityTypeBuilder<UsuarioRolEntity> builder)
        {
            builder.ToTable("usuarioxroles", "atracciones");
            builder.HasKey(x => x.UsuRolId);

            builder.Property(x => x.UsuRolId)
                .HasColumnName("usu_rol_id")
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.UsuId).HasColumnName("usu_id").IsRequired();
            builder.Property(x => x.RolId).HasColumnName("rol_id").IsRequired();

            builder.Property(x => x.UsuRolEstado)
                .HasColumnName("usu_rol_estado")
                .HasColumnType("char(1)")
                .HasDefaultValue('A')
                .IsRequired();

            builder.HasIndex(x => new { x.UsuId, x.RolId })
                .IsUnique()
                .HasDatabaseName("uk_usuarioxroles_par");

            builder.HasOne(x => x.UsuEntity)
                .WithMany(u => u.UsuariosRoles)
                .HasForeignKey(x => x.UsuId)
                .HasConstraintName("fk_usuarioxroles_usuario");

            builder.HasOne(x => x.RolEntity)
                .WithMany(r => r.UsuariosRoles)
                .HasForeignKey(x => x.RolId)
                .HasConstraintName("fk_usuarioxroles_rol");
        }
    }
}
