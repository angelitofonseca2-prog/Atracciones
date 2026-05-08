using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class AtraccionConfiguration : IEntityTypeConfiguration<AtraccionEntity>
    {
        public void Configure(EntityTypeBuilder<AtraccionEntity> builder)
        {
            builder.ToTable("atraccion", "atracciones");
            builder.HasKey(x => x.AtId);

            builder.Property(x => x.AtId).HasColumnName("at_id").UseIdentityAlwaysColumn();
            builder.Property(x => x.AtGuid).HasColumnName("at_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
            builder.Property(x => x.DesId).HasColumnName("des_id").IsRequired();
            builder.Property(x => x.AtNumEstablecimiento).HasColumnName("at_num_establecimiento").HasMaxLength(30);
            builder.Property(x => x.AtNombre).HasColumnName("at_nombre").HasMaxLength(200).IsRequired();
            builder.Property(x => x.AtDescripcion).HasColumnName("at_descripcion").HasMaxLength(2000);
            builder.Property(x => x.AtTotalResenias).HasColumnName("at_total_resenias").HasDefaultValue(0).IsRequired();
            builder.Property(x => x.AtDireccion).HasColumnName("at_direccion").HasMaxLength(300);
            builder.Property(x => x.AtDuracionMinutos).HasColumnName("at_duracion_minutos");
            builder.Property(x => x.AtPuntoEncuentro).HasColumnName("at_punto_encuentro").HasMaxLength(300);
            builder.Property(x => x.AtPrecioReferencia).HasColumnName("at_precio_referencia").HasColumnType("numeric(10,2)");
            builder.Property(x => x.AtIncluyeAcompaniante).HasColumnName("at_incluye_acompaniante").HasDefaultValue(false).IsRequired();
            builder.Property(x => x.AtIncluyeTransporte).HasColumnName("at_incluye_transporte").HasDefaultValue(false).IsRequired();
            builder.Property(x => x.AtDisponible).HasColumnName("at_disponible").HasDefaultValue(true).IsRequired();

            builder.Property(x => x.AtFechaIngreso).HasColumnName("at_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'").IsRequired();
            builder.Property(x => x.AtUsuarioIngreso).HasColumnName("at_usuario_ingreso").HasMaxLength(100).IsRequired();
            builder.Property(x => x.AtIpIngreso).HasColumnName("at_ip_ingreso").HasMaxLength(45).IsRequired();

            builder.Property(x => x.AtFechaMod).HasColumnName("at_fecha_mod");
            builder.Property(x => x.AtUsuarioMod).HasColumnName("at_usuario_mod").HasMaxLength(100);
            builder.Property(x => x.AtIpMod).HasColumnName("at_ip_mod").HasMaxLength(45);

            builder.Property(x => x.AtFechaEliminacion).HasColumnName("at_fecha_eliminacion");
            builder.Property(x => x.AtUsuarioEliminacion).HasColumnName("at_usuario_eliminacion").HasMaxLength(100);
            builder.Property(x => x.AtIpEliminacion).HasColumnName("at_ip_eliminacion").HasMaxLength(45);

            builder.Property(x => x.AtEstado).HasColumnName("at_estado").HasColumnType("char(1)").HasDefaultValue('A').IsRequired();

            builder.HasIndex(x => x.AtGuid).IsUnique().HasDatabaseName("uk_atraccion_guid");

            builder.HasOne(x => x.Destino)
                .WithMany(d => d.Atracciones)
                .HasForeignKey(x => x.DesId)
                .HasConstraintName("fk_atraccion_destino");
        }
    }
}
