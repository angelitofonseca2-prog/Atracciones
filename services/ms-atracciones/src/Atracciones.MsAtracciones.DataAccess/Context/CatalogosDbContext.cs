using Atracciones.MsAtracciones.DataAccess.Entities.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsAtracciones.DataAccess.Context;

public sealed class CatalogosDbContext : DbContext
{
    public CatalogosDbContext(DbContextOptions<CatalogosDbContext> options)
        : base(options)
    {
    }

    public DbSet<DestinoEntity> Destinos => Set<DestinoEntity>();
    public DbSet<CategoriaEntity> Categorias => Set<CategoriaEntity>();
    public DbSet<IdiomaEntity> Idiomas => Set<IdiomaEntity>();
    public DbSet<IncluyeEntity> Incluyes => Set<IncluyeEntity>();
    public DbSet<ImagenEntity> Imagenes => Set<ImagenEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalogos");

        modelBuilder.Entity<DestinoEntity>(b =>
        {
            b.ToTable("destinos");
            b.HasKey(x => x.DesGuid);
            b.Property(x => x.DesGuid).HasColumnName("des_guid");
            b.Property(x => x.DesNombre).HasColumnName("des_nombre").HasMaxLength(150).IsRequired();
            b.Property(x => x.DesPais).HasColumnName("des_pais").HasMaxLength(100).IsRequired();
            b.Property(x => x.DesImagenUrl).HasColumnName("des_imagen_url").HasMaxLength(500);
            b.Property(x => x.DesFechaIngreso).HasColumnName("des_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            b.Property(x => x.DesUsuarioIngreso).HasColumnName("des_usuario_ingreso").HasMaxLength(100).IsRequired();
            b.Property(x => x.DesIpIngreso).HasColumnName("des_ip_ingreso").HasMaxLength(45).IsRequired();
            b.Property(x => x.DesFechaMod).HasColumnName("des_fecha_mod");
            b.Property(x => x.DesUsuarioMod).HasColumnName("des_usuario_mod").HasMaxLength(100);
            b.Property(x => x.DesIpMod).HasColumnName("des_ip_mod").HasMaxLength(45);
            b.Property(x => x.DesFechaEliminacion).HasColumnName("des_fecha_eliminacion");
            b.Property(x => x.DesUsuarioEliminacion).HasColumnName("des_usuario_eliminacion").HasMaxLength(100);
            b.Property(x => x.DesIpEliminacion).HasColumnName("des_ip_eliminacion").HasMaxLength(45);
            b.Property(x => x.DesEstado).HasColumnName("des_estado").HasColumnType("char(1)").HasDefaultValue('A');
        });

        modelBuilder.Entity<CategoriaEntity>(b =>
        {
            b.ToTable("categorias");
            b.HasKey(x => x.CatGuid);
            b.Property(x => x.CatGuid).HasColumnName("cat_guid");
            b.Property(x => x.CatParentGuid).HasColumnName("cat_parent_guid");
            b.Property(x => x.CatNombre).HasColumnName("cat_nombre").HasMaxLength(100).IsRequired();
            b.Property(x => x.CatFechaIngreso).HasColumnName("cat_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            b.Property(x => x.CatUsuarioIngreso).HasColumnName("cat_usuario_ingreso").HasMaxLength(100).IsRequired();
            b.Property(x => x.CatIpIngreso).HasColumnName("cat_ip_ingreso").HasMaxLength(45).IsRequired();
            b.Property(x => x.CatFechaMod).HasColumnName("cat_fecha_mod");
            b.Property(x => x.CatUsuarioMod).HasColumnName("cat_usuario_mod").HasMaxLength(100);
            b.Property(x => x.CatIpMod).HasColumnName("cat_ip_mod").HasMaxLength(45);
            b.Property(x => x.CatFechaEliminacion).HasColumnName("cat_fecha_eliminacion");
            b.Property(x => x.CatUsuarioEliminacion).HasColumnName("cat_usuario_eliminacion").HasMaxLength(100);
            b.Property(x => x.CatIpEliminacion).HasColumnName("cat_ip_eliminacion").HasMaxLength(45);
            b.Property(x => x.CatEstado).HasColumnName("cat_estado").HasColumnType("char(1)").HasDefaultValue('A');
            b.HasOne<CategoriaEntity>().WithMany().HasForeignKey(x => x.CatParentGuid).IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<IdiomaEntity>(b =>
        {
            b.ToTable("idiomas");
            b.HasKey(x => x.IdGuid);
            b.Property(x => x.IdGuid).HasColumnName("id_guid");
            b.Property(x => x.IdDescripcion).HasColumnName("id_descripcion").HasMaxLength(80).IsRequired();
            b.Property(x => x.IdFechaIngreso).HasColumnName("id_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            b.Property(x => x.IdUsuarioIngreso).HasColumnName("id_usuario_ingreso").HasMaxLength(100).IsRequired();
            b.Property(x => x.IdIpIngreso).HasColumnName("id_ip_ingreso").HasMaxLength(45).IsRequired();
            b.Property(x => x.IdFechaMod).HasColumnName("id_fecha_mod");
            b.Property(x => x.IdUsuarioMod).HasColumnName("id_usuario_mod").HasMaxLength(100);
            b.Property(x => x.IdIpMod).HasColumnName("id_ip_mod").HasMaxLength(45);
            b.Property(x => x.IdFechaEliminacion).HasColumnName("id_fecha_eliminacion");
            b.Property(x => x.IdUsuarioEliminacion).HasColumnName("id_usuario_eliminacion").HasMaxLength(100);
            b.Property(x => x.IdIpEliminacion).HasColumnName("id_ip_eliminacion").HasMaxLength(45);
            b.Property(x => x.IdEstado).HasColumnName("id_estado").HasColumnType("char(1)").HasDefaultValue('A');
            b.HasIndex(x => x.IdDescripcion).IsUnique().HasDatabaseName("uk_idiomas_descripcion");
        });

        modelBuilder.Entity<IncluyeEntity>(b =>
        {
            b.ToTable("incluye");
            b.HasKey(x => x.IncGuid);
            b.Property(x => x.IncGuid).HasColumnName("inc_guid");
            b.Property(x => x.IncDescripcion).HasColumnName("inc_descripcion").HasMaxLength(200).IsRequired();
            b.Property(x => x.IncEstado).HasColumnName("inc_estado").HasColumnType("char(1)").HasDefaultValue('A');
        });

        modelBuilder.Entity<ImagenEntity>(b =>
        {
            b.ToTable("imagenes");
            b.HasKey(x => x.ImgGuid);
            b.Property(x => x.ImgGuid).HasColumnName("img_guid");
            b.Property(x => x.ImgUrl).HasColumnName("img_url").HasMaxLength(500).IsRequired();
            b.Property(x => x.ImgDescripcion).HasColumnName("img_descripcion").HasMaxLength(200);
            b.Property(x => x.ImgFechaIngreso).HasColumnName("img_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            b.Property(x => x.ImgUsuarioIngreso).HasColumnName("img_usuario_ingreso").HasMaxLength(100).IsRequired();
            b.Property(x => x.ImgIpIngreso).HasColumnName("img_ip_ingreso").HasMaxLength(45).IsRequired();
            b.Property(x => x.ImgFechaMod).HasColumnName("img_fecha_mod");
            b.Property(x => x.ImgUsuarioMod).HasColumnName("img_usuario_mod").HasMaxLength(100);
            b.Property(x => x.ImgIpMod).HasColumnName("img_ip_mod").HasMaxLength(45);
            b.Property(x => x.ImgFechaEliminacion).HasColumnName("img_fecha_eliminacion");
            b.Property(x => x.ImgUsuarioEliminacion).HasColumnName("img_usuario_eliminacion").HasMaxLength(100);
            b.Property(x => x.ImgIpEliminacion).HasColumnName("img_ip_eliminacion").HasMaxLength(45);
            b.Property(x => x.ImgEstado).HasColumnName("img_estado").HasColumnType("char(1)").HasDefaultValue('A');
        });
    }
}
