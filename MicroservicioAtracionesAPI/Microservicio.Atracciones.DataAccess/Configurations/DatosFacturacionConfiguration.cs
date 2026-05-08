using Microservicio.Atracciones.DataAccess.Entities.Facturacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class DatosFacturacionConfiguration : IEntityTypeConfiguration<DatosFacturacionEntity>
    {
        public void Configure(EntityTypeBuilder<DatosFacturacionEntity> builder)
        {
            builder.ToTable("datos_facturacion", "atracciones");
            builder.HasKey(x => x.DfacId);

            builder.Property(x => x.DfacId).HasColumnName("dfac_id").UseIdentityAlwaysColumn();
            builder.Property(x => x.DfacGuid).HasColumnName("dfac_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
            builder.Property(x => x.FacId).HasColumnName("fac_id").IsRequired();
            builder.Property(x => x.DfacNombre).HasColumnName("dfac_nombre").HasMaxLength(100).IsRequired();
            builder.Property(x => x.DfacApellido).HasColumnName("dfac_apellido").HasMaxLength(100);
            builder.Property(x => x.DfacCorreo).HasColumnName("dfac_correo").HasMaxLength(150).IsRequired();
            builder.Property(x => x.DfacTelefono).HasColumnName("dfac_telefono").HasMaxLength(20);

            builder.HasIndex(x => x.DfacGuid).IsUnique().HasDatabaseName("uk_datos_facturacion_guid");
            // 1 registro por factura
            builder.HasIndex(x => x.FacId).IsUnique().HasDatabaseName("uk_datos_facturacion_fac");

            builder.HasOne(x => x.Factura)
                .WithOne(f => f.DatosFacturacion)
                .HasForeignKey<DatosFacturacionEntity>(x => x.FacId)
                .HasConstraintName("fk_datos_facturacion_fac");
        }
    }

}
