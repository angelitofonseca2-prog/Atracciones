using Microservicio.Atracciones.DataAccess.Entities.Auditoria;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class AuditoriaLogConfiguration : IEntityTypeConfiguration<AuditoriaLogEntity>
    {
        public void Configure(EntityTypeBuilder<AuditoriaLogEntity> builder)
        {
            builder.ToTable("auditoria_log", "atracciones");
            builder.HasKey(x => x.LogId);

            builder.Property(x => x.LogId).HasColumnName("log_id").UseIdentityAlwaysColumn();
            builder.Property(x => x.LogGuid).HasColumnName("log_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();

            builder.Property(x => x.LogTabla).HasColumnName("log_tabla").HasMaxLength(100).IsRequired();
            builder.Property(x => x.LogOperacion).HasColumnName("log_operacion").HasMaxLength(20).IsRequired();
            builder.Property(x => x.LogRegistroId).HasColumnName("log_registro_id");
            builder.Property(x => x.LogRegistroGuid).HasColumnName("log_registro_guid");

            // TEXT = NVARCHAR(MAX) → snapshots JSON ilimitados
            builder.Property(x => x.LogDatosAnteriores).HasColumnName("log_datos_anteriores").HasColumnType("text");
            builder.Property(x => x.LogDatosNuevos).HasColumnName("log_datos_nuevos").HasColumnType("text");

            builder.Property(x => x.LogFechaUtc)
                .HasColumnName("log_fecha_utc")
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'")
                .IsRequired();

            builder.Property(x => x.LogUsuario).HasColumnName("log_usuario").HasMaxLength(100).IsRequired();
            builder.Property(x => x.LogIp).HasColumnName("log_ip").HasMaxLength(45).IsRequired();
            builder.Property(x => x.LogOrigenCanal).HasColumnName("log_origen_canal").HasMaxLength(200);

            builder.HasIndex(x => x.LogGuid).IsUnique().HasDatabaseName("uk_auditoria_log_guid");

            // Sin FK físicas — referencia lógica para sobrevivir soft-deletes
        }
    }
}
