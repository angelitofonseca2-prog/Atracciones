using Atracciones.MsClientes.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsClientes.DataAccess.Context;

public sealed class CrmDbContext : DbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options)
        : base(options)
    {
    }

    public DbSet<ClienteEntity> Clientes => Set<ClienteEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("crm");

        modelBuilder.Entity<ClienteEntity>(b =>
        {
            b.ToTable("clientes");
            b.HasKey(x => x.CliGuid);
            b.Property(x => x.CliGuid).HasColumnName("cli_guid");
            b.Property(x => x.CliTipoIdentificacion).HasColumnName("cli_tipo_identificacion").HasMaxLength(20).IsRequired();
            b.Property(x => x.CliNumeroIdentificacion).HasColumnName("cli_numero_identificacion").HasMaxLength(20).IsRequired();
            b.Property(x => x.CliNombres).HasColumnName("cli_nombres").HasMaxLength(100);
            b.Property(x => x.CliApellidos).HasColumnName("cli_apellidos").HasMaxLength(100);
            b.Property(x => x.CliRazonSocial).HasColumnName("cli_razon_social").HasMaxLength(200);
            b.Property(x => x.CliCorreo).HasColumnName("cli_correo").HasMaxLength(150).IsRequired();
            b.Property(x => x.CliTelefono).HasColumnName("cli_telefono").HasMaxLength(20);
            b.Property(x => x.CliDireccion).HasColumnName("cli_direccion").HasMaxLength(300);
            b.Property(x => x.CliEstado).HasColumnName("cli_estado").HasMaxLength(1).IsFixedLength().HasColumnType("char(1)").HasDefaultValue('A');
            b.Property(x => x.CliFechaIngreso).HasColumnName("cli_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            b.Property(x => x.CliUsuarioIngreso).HasColumnName("cli_usuario_ingreso").HasMaxLength(100).IsRequired();
            b.Property(x => x.CliIpIngreso).HasColumnName("cli_ip_ingreso").HasMaxLength(45).IsRequired();
            b.HasIndex(x => x.CliNumeroIdentificacion).IsUnique();
        });
    }
}
