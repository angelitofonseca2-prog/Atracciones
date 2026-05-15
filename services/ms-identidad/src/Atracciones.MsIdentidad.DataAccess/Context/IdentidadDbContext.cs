using Atracciones.MsIdentidad.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsIdentidad.DataAccess.Context;

public sealed class IdentidadDbContext : DbContext
{
    public IdentidadDbContext(DbContextOptions<IdentidadDbContext> options)
        : base(options)
    {
    }

    public DbSet<UsuarioEntity> Usuarios => Set<UsuarioEntity>();
    public DbSet<RolEntity> Roles => Set<RolEntity>();
    public DbSet<UsuarioRolEntity> UsuarioRoles => Set<UsuarioRolEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");

        modelBuilder.Entity<RolEntity>(b =>
        {
            b.ToTable("roles");
            b.HasKey(x => x.RolId);
            b.Property(x => x.RolId).HasColumnName("rol_id").UseIdentityByDefaultColumn();
            b.Property(x => x.RolGuid).HasColumnName("rol_guid").HasDefaultValueSql("gen_random_uuid()");
            b.Property(x => x.RolDescripcion).HasColumnName("rol_descripcion").HasMaxLength(80).IsRequired();
            b.Property(x => x.RolEstado).HasColumnName("rol_estado").HasMaxLength(1).IsFixedLength().HasColumnType("char(1)").HasDefaultValue('A');
            b.HasIndex(x => x.RolDescripcion).IsUnique();
        });

        modelBuilder.Entity<UsuarioEntity>(b =>
        {
            b.ToTable("usuarios");
            b.HasKey(x => x.UsuId);
            b.Property(x => x.UsuId).HasColumnName("usu_id").UseIdentityByDefaultColumn();
            b.Property(x => x.UsuGuid).HasColumnName("usu_guid").HasDefaultValueSql("gen_random_uuid()");
            b.Property(x => x.UsuLogin).HasColumnName("usu_login").HasMaxLength(100).IsRequired();
            b.Property(x => x.UsuPasswordHash).HasColumnName("usu_password_hash").HasMaxLength(256).IsRequired();
            b.Property(x => x.UsuFechaRegistro).HasColumnName("usu_fecha_registro").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            b.Property(x => x.UsuUsuarioRegistro).HasColumnName("usu_usuario_registro").HasMaxLength(100).IsRequired();
            b.Property(x => x.UsuIpRegistro).HasColumnName("usu_ip_registro").HasMaxLength(45).IsRequired();
            b.Property(x => x.UsuEstado).HasColumnName("usu_estado").HasMaxLength(1).IsFixedLength().HasColumnType("char(1)").HasDefaultValue('A');
            b.Property(x => x.CliId).HasColumnName("cli_id");
            b.HasIndex(x => x.UsuGuid).IsUnique();
            b.HasIndex(x => x.UsuLogin).IsUnique();
        });

        modelBuilder.Entity<UsuarioRolEntity>(b =>
        {
            b.ToTable("usuario_roles");
            b.HasKey(x => new { x.UsuId, x.RolId });
            b.Property(x => x.UsuId).HasColumnName("usu_id");
            b.Property(x => x.RolId).HasColumnName("rol_id");
            b.Property(x => x.UsuRolEstado).HasColumnName("usu_rol_estado").HasMaxLength(1).IsFixedLength().HasColumnType("char(1)").HasDefaultValue('A');
            b.HasOne(x => x.Usuario).WithMany(x => x.UsuarioRoles).HasForeignKey(x => x.UsuId);
            b.HasOne(x => x.Rol).WithMany(x => x.UsuarioRoles).HasForeignKey(x => x.RolId);
        });
    }
}
