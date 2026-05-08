using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class DestinoConfiguration : IEntityTypeConfiguration<DestinoEntity>
    {
        public void Configure(EntityTypeBuilder<DestinoEntity> builder)
        {
            builder.ToTable("destino", "atracciones");
            builder.HasKey(x => x.DesId);

            builder.Property(x => x.DesId).HasColumnName("des_id").UseIdentityAlwaysColumn();
            builder.Property(x => x.DesGuid).HasColumnName("des_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
            builder.Property(x => x.DesNombre).HasColumnName("des_nombre").HasMaxLength(150).IsRequired();
            builder.Property(x => x.DesPais).HasColumnName("des_pais").HasMaxLength(100).IsRequired();
            builder.Property(x => x.DesImagenUrl).HasColumnName("des_imagen_url").HasMaxLength(500);

            builder.Property(x => x.DesFechaIngreso).HasColumnName("des_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'").IsRequired();
            builder.Property(x => x.DesUsuarioIngreso).HasColumnName("des_usuario_ingreso").HasMaxLength(100).IsRequired();
            builder.Property(x => x.DesIpIngreso).HasColumnName("des_ip_ingreso").HasMaxLength(45).IsRequired();

            builder.Property(x => x.DesFechaMod).HasColumnName("des_fecha_mod");
            builder.Property(x => x.DesUsuarioMod).HasColumnName("des_usuario_mod").HasMaxLength(100);
            builder.Property(x => x.DesIpMod).HasColumnName("des_ip_mod").HasMaxLength(45);

            builder.Property(x => x.DesFechaEliminacion).HasColumnName("des_fecha_eliminacion");
            builder.Property(x => x.DesUsuarioEliminacion).HasColumnName("des_usuario_eliminacion").HasMaxLength(100);
            builder.Property(x => x.DesIpEliminacion).HasColumnName("des_ip_eliminacion").HasMaxLength(45);

            builder.Property(x => x.DesEstado).HasColumnName("des_estado").HasColumnType("char(1)").HasDefaultValue('A').IsRequired();

            builder.HasIndex(x => x.DesGuid).IsUnique().HasDatabaseName("uk_destino_guid");
        }
    }
}
