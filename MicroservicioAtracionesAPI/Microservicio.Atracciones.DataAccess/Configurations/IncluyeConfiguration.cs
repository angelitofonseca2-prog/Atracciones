using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class IncluyeConfiguration : IEntityTypeConfiguration<IncluyeEntity>
    {
        public void Configure(EntityTypeBuilder<IncluyeEntity> builder)
        {
            builder.ToTable("incluye", "atracciones");
            builder.HasKey(x => x.IncId);

            builder.Property(x => x.IncId).HasColumnName("inc_id").UseIdentityAlwaysColumn();
            builder.Property(x => x.IncGuid).HasColumnName("inc_guid").HasDefaultValueSql("gen_random_uuid()").IsRequired();
            builder.Property(x => x.IncDescripcion).HasColumnName("inc_descripcion").HasMaxLength(200).IsRequired();
            builder.Property(x => x.IncEstado).HasColumnName("inc_estado").HasColumnType("char(1)").HasDefaultValue('A').IsRequired();

            builder.HasIndex(x => x.IncGuid).IsUnique().HasDatabaseName("uk_incluye_guid");
        }
    }
}
