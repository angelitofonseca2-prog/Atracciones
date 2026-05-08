using Microservicio.Atracciones.DataAccess.Entities.Clientes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microservicio.Atracciones.DataAccess.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<ClienteEntity>
    {
        public void Configure(EntityTypeBuilder<ClienteEntity> builder)
        {
            builder.ToTable("clientes", "atracciones");
            builder.HasKey(x => x.CliId);

            builder.Property(x => x.CliId)
                .HasColumnName("cli_id")
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.CliGuid)
                .HasColumnName("cli_guid")
                .HasDefaultValueSql("gen_random_uuid()")
                .IsRequired();

            builder.Property(x => x.UsuId).HasColumnName("usu_id").IsRequired(false);

            builder.Property(x => x.CliTipoIdentificacion)
                .HasColumnName("cli_tipo_identificacion")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.CliNumeroIdentificacion)
                .HasColumnName("cli_numero_identificacion")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.CliNombres)
                .HasColumnName("cli_nombres")
                .HasMaxLength(100);

            builder.Property(x => x.CliApellidos)
                .HasColumnName("cli_apellidos")
                .HasMaxLength(100);

            builder.Property(x => x.CliRazonSocial)
                .HasColumnName("cli_razon_social")
                .HasMaxLength(200);

            builder.Property(x => x.CliCorreo)
                .HasColumnName("cli_correo")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.CliTelefono)
                .HasColumnName("cli_telefono")
                .HasMaxLength(20);

            builder.Property(x => x.CliDireccion)
                .HasColumnName("cli_direccion")
                .HasMaxLength(300);

            builder.Property(x => x.CliFechaIngreso)
                .HasColumnName("cli_fecha_ingreso")
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'")
                .IsRequired();

            builder.Property(x => x.CliUsuarioIngreso)
                .HasColumnName("cli_usuario_ingreso")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.CliIpIngreso)
                .HasColumnName("cli_ip_ingreso")
                .HasMaxLength(45)
                .IsRequired();

            builder.Property(x => x.CliFechaEliminacion).HasColumnName("cli_fecha_eliminacion");
            builder.Property(x => x.CliUsuarioEliminacion).HasColumnName("cli_usuario_eliminacion").HasMaxLength(100);
            builder.Property(x => x.CliIpEliminacion).HasColumnName("cli_ip_eliminacion").HasMaxLength(45);

            builder.Property(x => x.CliEstado)
                .HasColumnName("cli_estado")
                .HasColumnType("char(1)")
                .HasDefaultValue('A')
                .IsRequired();

            builder.HasIndex(x => x.CliGuid).IsUnique().HasDatabaseName("uk_clientes_guid");
            builder.HasIndex(x => x.CliNumeroIdentificacion).IsUnique().HasDatabaseName("uk_clientes_num_identificacion");

            builder.HasOne(x => x.Usuario)
                .WithOne(u => u.Cliente)
                .HasForeignKey<ClienteEntity>(x => x.UsuId)
                .HasConstraintName("fk_clientes_usuario");
        }
    }
}
