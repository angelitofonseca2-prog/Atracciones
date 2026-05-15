using Atracciones.MsAtracciones.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsAtracciones.DataAccess.Context;

public sealed class InventarioDbContext : DbContext
{
    public InventarioDbContext(DbContextOptions<InventarioDbContext> options)
        : base(options)
    {
    }

    public DbSet<AtraccionEntity> Atracciones => Set<AtraccionEntity>();
    public DbSet<TicketEntity> Tickets => Set<TicketEntity>();
    public DbSet<HorarioEntity> Horarios => Set<HorarioEntity>();
    public DbSet<AtraccionCategoriaEntity> AtraccionCategorias => Set<AtraccionCategoriaEntity>();
    public DbSet<AtraccionIdiomaEntity> AtraccionIdiomas => Set<AtraccionIdiomaEntity>();
    public DbSet<AtraccionImagenEntity> AtraccionImagenes => Set<AtraccionImagenEntity>();
    public DbSet<AtraccionIncluyeEntity> AtraccionIncluyes => Set<AtraccionIncluyeEntity>();
    public DbSet<ReseniaEntity> Resenias => Set<ReseniaEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inventario");

        modelBuilder.Entity<AtraccionEntity>(b =>
        {
            b.ToTable("atracciones");
            b.HasKey(x => x.AtGuid);
            b.Property(x => x.AtGuid).HasColumnName("at_guid");
            b.Property(x => x.DesGuid).HasColumnName("des_guid").IsRequired();
            b.Property(x => x.DesNombreSnap).HasColumnName("des_nombre_snap").HasMaxLength(150).IsRequired();
            b.Property(x => x.DesPaisSnap).HasColumnName("des_pais_snap").HasMaxLength(100).IsRequired();
            b.Property(x => x.AtNumEstablecimiento).HasColumnName("at_num_establecimiento").HasMaxLength(30);
            b.Property(x => x.AtNombre).HasColumnName("at_nombre").HasMaxLength(200).IsRequired();
            b.Property(x => x.AtDescripcion).HasColumnName("at_descripcion").HasMaxLength(2000);
            b.Property(x => x.AtTotalResenias).HasColumnName("at_total_resenias").HasDefaultValue(0);
            b.Property(x => x.AtDireccion).HasColumnName("at_direccion").HasMaxLength(300);
            b.Property(x => x.AtDuracionMinutos).HasColumnName("at_duracion_minutos");
            b.Property(x => x.AtPuntoEncuentro).HasColumnName("at_punto_encuentro").HasMaxLength(300);
            b.Property(x => x.AtPrecioReferencia).HasColumnName("at_precio_referencia").HasColumnType("numeric(10,2)");
            b.Property(x => x.AtIncluyeAcompaniante).HasColumnName("at_incluye_acompaniante").HasDefaultValue(false);
            b.Property(x => x.AtIncluyeTransporte).HasColumnName("at_incluye_transporte").HasDefaultValue(false);
            b.Property(x => x.AtDisponible).HasColumnName("at_disponible").HasDefaultValue(true);
            b.Property(x => x.AtFechaIngreso).HasColumnName("at_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            b.Property(x => x.AtUsuarioIngreso).HasColumnName("at_usuario_ingreso").HasMaxLength(100).IsRequired();
            b.Property(x => x.AtIpIngreso).HasColumnName("at_ip_ingreso").HasMaxLength(45).IsRequired();
            b.Property(x => x.AtFechaMod).HasColumnName("at_fecha_mod");
            b.Property(x => x.AtUsuarioMod).HasColumnName("at_usuario_mod").HasMaxLength(100);
            b.Property(x => x.AtIpMod).HasColumnName("at_ip_mod").HasMaxLength(45);
            b.Property(x => x.AtFechaEliminacion).HasColumnName("at_fecha_eliminacion");
            b.Property(x => x.AtUsuarioEliminacion).HasColumnName("at_usuario_eliminacion").HasMaxLength(100);
            b.Property(x => x.AtIpEliminacion).HasColumnName("at_ip_eliminacion").HasMaxLength(45);
            b.Property(x => x.AtEstado).HasColumnName("at_estado").HasColumnType("char(1)").HasDefaultValue('A');
        });

        modelBuilder.Entity<TicketEntity>(b =>
        {
            b.ToTable("tickets");
            b.HasKey(x => x.TckGuid);
            b.Property(x => x.TckGuid).HasColumnName("tck_guid");
            b.Property(x => x.AtGuid).HasColumnName("at_guid").IsRequired();
            b.Property(x => x.TckTitulo).HasColumnName("tck_titulo").HasMaxLength(150).IsRequired();
            b.Property(x => x.TckPrecio).HasColumnName("tck_precio").HasColumnType("numeric(10,2)").IsRequired();
            b.Property(x => x.TckTipoParticipante).HasColumnName("tck_tipo_participante").HasMaxLength(30).HasDefaultValue("Adulto");
            b.Property(x => x.TckCapacidadMaxima).HasColumnName("tck_capacidad_maxima").IsRequired();
            b.Property(x => x.TckCuposDisponibles).HasColumnName("tck_cupos_disponibles").IsRequired();
            b.Property(x => x.TckFechaIngreso).HasColumnName("tck_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            b.Property(x => x.TckUsuarioIngreso).HasColumnName("tck_usuario_ingreso").HasMaxLength(100).IsRequired();
            b.Property(x => x.TckIpIngreso).HasColumnName("tck_ip_ingreso").HasMaxLength(45).IsRequired();
            b.Property(x => x.TckFechaMod).HasColumnName("tck_fecha_mod");
            b.Property(x => x.TckUsuarioMod).HasColumnName("tck_usuario_mod").HasMaxLength(100);
            b.Property(x => x.TckIpMod).HasColumnName("tck_ip_mod").HasMaxLength(45);
            b.Property(x => x.TckFechaEliminacion).HasColumnName("tck_fecha_eliminacion");
            b.Property(x => x.TckUsuarioEliminacion).HasColumnName("tck_usuario_eliminacion").HasMaxLength(100);
            b.Property(x => x.TckIpEliminacion).HasColumnName("tck_ip_eliminacion").HasMaxLength(45);
            b.Property(x => x.TckEstado).HasColumnName("tck_estado").HasColumnType("char(1)").HasDefaultValue('A');
            b.HasOne(x => x.Atraccion).WithMany(x => x.Tickets).HasForeignKey(x => x.AtGuid).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HorarioEntity>(b =>
        {
            b.ToTable("horarios");
            b.HasKey(x => x.HorGuid);
            b.Property(x => x.HorGuid).HasColumnName("hor_guid");
            b.Property(x => x.TckGuid).HasColumnName("tck_guid").IsRequired();
            b.Property(x => x.HorFecha).HasColumnName("hor_fecha").IsRequired();
            b.Property(x => x.HorHoraInicio).HasColumnName("hor_hora_inicio").IsRequired();
            b.Property(x => x.HorHoraFin).HasColumnName("hor_hora_fin");
            b.Property(x => x.HorCuposDisponibles).HasColumnName("hor_cupos_disponibles").IsRequired();
            b.Property(x => x.HorFechaIngreso).HasColumnName("hor_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            b.Property(x => x.HorUsuarioIngreso).HasColumnName("hor_usuario_ingreso").HasMaxLength(100).IsRequired();
            b.Property(x => x.HorIpIngreso).HasColumnName("hor_ip_ingreso").HasMaxLength(45).IsRequired();
            b.Property(x => x.HorFechaMod).HasColumnName("hor_fecha_mod");
            b.Property(x => x.HorUsuarioMod).HasColumnName("hor_usuario_mod").HasMaxLength(100);
            b.Property(x => x.HorIpMod).HasColumnName("hor_ip_mod").HasMaxLength(45);
            b.Property(x => x.HorFechaEliminacion).HasColumnName("hor_fecha_eliminacion");
            b.Property(x => x.HorUsuarioEliminacion).HasColumnName("hor_usuario_eliminacion").HasMaxLength(100);
            b.Property(x => x.HorIpEliminacion).HasColumnName("hor_ip_eliminacion").HasMaxLength(45);
            b.Property(x => x.HorEstado).HasColumnName("hor_estado").HasColumnType("char(1)").HasDefaultValue('A');
            b.HasOne(x => x.Ticket).WithMany(x => x.Horarios).HasForeignKey(x => x.TckGuid).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TckGuid, x.HorFecha, x.HorHoraInicio }).IsUnique().HasDatabaseName("uk_horario_slot");
        });

        modelBuilder.Entity<AtraccionCategoriaEntity>(b =>
        {
            b.ToTable("atraccion_categoria");
            b.HasKey(x => new { x.AtGuid, x.CatGuid });
            b.Property(x => x.AtGuid).HasColumnName("at_guid");
            b.Property(x => x.CatGuid).HasColumnName("cat_guid");
            b.Property(x => x.CaEstado).HasColumnName("ca_estado").HasColumnType("char(1)").HasDefaultValue('A');
            b.Property(x => x.CaFechaIngreso).HasColumnName("ca_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            b.Property(x => x.CaUsuarioIngreso).HasColumnName("ca_usuario_ingreso").HasMaxLength(100).IsRequired();
            b.HasOne(x => x.Atraccion).WithMany(x => x.Categorias).HasForeignKey(x => x.AtGuid).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AtraccionIdiomaEntity>(b =>
        {
            b.ToTable("atraccion_idioma");
            b.HasKey(x => new { x.AtGuid, x.IdGuid });
            b.Property(x => x.IdDescripcionSnap).HasColumnName("id_descripcion_snap").HasMaxLength(80).IsRequired();
            b.Property(x => x.IaEstado).HasColumnName("ia_estado").HasColumnType("char(1)").HasDefaultValue('A');
            b.Property(x => x.IaFechaIngreso).HasColumnName("ia_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            b.Property(x => x.IaUsuarioIngreso).HasColumnName("ia_usuario_ingreso").HasMaxLength(100).IsRequired();
            b.HasOne(x => x.Atraccion).WithMany(x => x.Idiomas).HasForeignKey(x => x.AtGuid).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AtraccionImagenEntity>(b =>
        {
            b.ToTable("atraccion_imagen");
            b.HasKey(x => new { x.AtGuid, x.ImgGuid });
            b.Property(x => x.ImgUrlSnap).HasColumnName("img_url_snap").HasMaxLength(500).IsRequired();
            b.Property(x => x.ImaOrden).HasColumnName("ima_orden").IsRequired();
            b.Property(x => x.ImaEstado).HasColumnName("ima_estado").HasColumnType("char(1)").HasDefaultValue('A');
            b.Property(x => x.ImaFechaIngreso).HasColumnName("ima_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            b.Property(x => x.ImaUsuarioIngreso).HasColumnName("ima_usuario_ingreso").HasMaxLength(100).IsRequired();
            b.HasOne(x => x.Atraccion).WithMany(x => x.Imagenes).HasForeignKey(x => x.AtGuid).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AtraccionIncluyeEntity>(b =>
        {
            b.ToTable("atraccion_incluye");
            b.HasKey(x => new { x.AtGuid, x.IncGuid });
            b.Property(x => x.IncDescripcionSnap).HasColumnName("inc_descripcion_snap").HasMaxLength(200).IsRequired();
            b.Property(x => x.AiEstado).HasColumnName("ai_estado").HasColumnType("char(1)").HasDefaultValue('A');
            b.Property(x => x.AiFechaIngreso).HasColumnName("ai_fecha_ingreso").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            b.Property(x => x.AiUsuarioIngreso).HasColumnName("ai_usuario_ingreso").HasMaxLength(100).IsRequired();
            b.HasOne(x => x.Atraccion).WithMany(x => x.Incluyes).HasForeignKey(x => x.AtGuid).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReseniaEntity>(b =>
        {
            b.ToTable("resenias");
            b.HasKey(x => x.RsnGuid);
            b.Property(x => x.RsnGuid).HasColumnName("rsn_guid");
            b.Property(x => x.AtGuid).HasColumnName("at_guid").IsRequired();
            b.Property(x => x.RevGuid).HasColumnName("rev_guid").IsRequired();
            b.Property(x => x.RsnComentario).HasColumnName("rsn_comentario").HasMaxLength(1000);
            b.Property(x => x.RsnRating).HasColumnName("rsn_rating").HasColumnType("numeric(3,2)").IsRequired();
            b.Property(x => x.RsnFechaCreacion).HasColumnName("rsn_fecha_creacion").HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            b.Property(x => x.RsnUsuarioCreacion).HasColumnName("rsn_usuario_creacion").HasMaxLength(100).IsRequired();
            b.Property(x => x.RsnIpCreacion).HasColumnName("rsn_ip_creacion").HasMaxLength(45).IsRequired();
            b.Property(x => x.RsnEstado).HasColumnName("rsn_estado").HasColumnType("char(1)").HasDefaultValue('A');
            b.HasIndex(x => x.RevGuid).IsUnique().HasDatabaseName("uk_resenia_rev_guid");
            b.HasOne(x => x.Atraccion).WithMany(x => x.Resenias).HasForeignKey(x => x.AtGuid).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
