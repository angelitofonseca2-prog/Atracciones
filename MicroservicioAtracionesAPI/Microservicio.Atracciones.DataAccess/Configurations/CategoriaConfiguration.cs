using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class CategoriaConfiguration : IEntityTypeConfiguration<CategoriaEntity>
    {
        public void Configure(EntityTypeBuilder<CategoriaEntity> builder)
        {
            builder.ToTable("categoria", "atracciones");
            builder.HasKey(x => x.CatId);

            builder.Property(x => x.CatId).HasColumnName("cat_id").UseIdentityAlwaysColumn();
            builder.Property(x => x.CatGuid).HasColumnName("cat_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
            builder.Property(x => x.CatParentId).HasColumnName("cat_parent_id");
            builder.Property(x => x.CatNombre).HasColumnName("cat_nombre").HasMaxLength(100).IsRequired();

            builder.Property(x => x.CatFechaIngreso).HasColumnName("cat_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'").IsRequired();
            builder.Property(x => x.CatUsuarioIngreso).HasColumnName("cat_usuario_ingreso").HasMaxLength(100).IsRequired();
            builder.Property(x => x.CatIpIngreso).HasColumnName("cat_ip_ingreso").HasMaxLength(45).IsRequired();

            builder.Property(x => x.CatFechaMod).HasColumnName("cat_fecha_mod");
            builder.Property(x => x.CatUsuarioMod).HasColumnName("cat_usuario_mod").HasMaxLength(100);
            builder.Property(x => x.CatIpMod).HasColumnName("cat_ip_mod").HasMaxLength(45);

            builder.Property(x => x.CatFechaEliminacion).HasColumnName("cat_fecha_eliminacion");
            builder.Property(x => x.CatUsuarioEliminacion).HasColumnName("cat_usuario_eliminacion").HasMaxLength(100);
            builder.Property(x => x.CatIpEliminacion).HasColumnName("cat_ip_eliminacion").HasMaxLength(45);

            builder.Property(x => x.CatEstado).HasColumnName("cat_estado").HasColumnType("char(1)").HasDefaultValue('A').IsRequired();

            builder.HasIndex(x => x.CatGuid).IsUnique().HasDatabaseName("uk_categoria_guid");

            // Auto-referencia padre → hijos
            builder.HasOne(x => x.Parent)
                .WithMany(c => c.Hijos)
                .HasForeignKey(x => x.CatParentId)
                .HasConstraintName("fk_categoria_parent")
                .IsRequired(false);
        }
    }
}
