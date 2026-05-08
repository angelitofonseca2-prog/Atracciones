using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class CategoriaAtraccionConfiguration : IEntityTypeConfiguration<CategoriaAtraccionEntity>
    {
        public void Configure(EntityTypeBuilder<CategoriaAtraccionEntity> builder)
        {
            builder.ToTable("categoria_atraccion", "atracciones");
            builder.HasKey(x => new { x.CatId, x.AtId });

            builder.Property(x => x.CatId).HasColumnName("cat_id").IsRequired();
            builder.Property(x => x.AtId).HasColumnName("at_id").IsRequired();
            builder.Property(x => x.CaFechaIngreso).HasColumnName("ca_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'").IsRequired();
            builder.Property(x => x.CaUsuarioIngreso).HasColumnName("ca_usuario_ingreso").HasMaxLength(100).IsRequired();
            builder.Property(x => x.CaFechaEliminacion).HasColumnName("ca_fecha_eliminacion");
            builder.Property(x => x.CaUsuarioEliminacion).HasColumnName("ca_usuario_eliminacion").HasMaxLength(100);
            builder.Property(x => x.CaEstado).HasColumnName("ca_estado").HasColumnType("char(1)").HasDefaultValue('A').IsRequired();

            builder.HasOne(x => x.Categoria)
                .WithMany(c => c.CategoriasAtracciones)
                .HasForeignKey(x => x.CatId)
                .HasConstraintName("fk_ca_categoria");

            builder.HasOne(x => x.Atraccion)
                .WithMany(a => a.CategoriasAtracciones)
                .HasForeignKey(x => x.AtId)
                .HasConstraintName("fk_ca_atraccion");
        }
    }
}
