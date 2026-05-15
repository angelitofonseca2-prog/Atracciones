using Atracciones.MsAuditoria.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsAuditoria.DataAccess.Context;

public sealed class AuditoriaDbContext : DbContext
{
    public AuditoriaDbContext(DbContextOptions<AuditoriaDbContext> options)
        : base(options)
    {
    }

    public DbSet<EventoAuditoriaEntity> Eventos => Set<EventoAuditoriaEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("audit");

        modelBuilder.Entity<EventoAuditoriaEntity>(b =>
        {
            b.ToTable("eventos");
            b.HasKey(x => x.EvtGuid);
            b.Property(x => x.EvtGuid).HasColumnName("evt_guid");
            b.Property(x => x.EvtTipo).HasColumnName("evt_tipo").HasMaxLength(120).IsRequired();
            b.Property(x => x.CorrelationId).HasColumnName("correlation_id").HasMaxLength(128).IsRequired();
            b.Property(x => x.PayloadJson).HasColumnName("payload_json").HasColumnType("text").IsRequired();
            b.Property(x => x.FechaUtc).HasColumnName("fecha_utc");
            b.HasIndex(x => x.CorrelationId);
            b.HasIndex(x => x.FechaUtc);
            b.HasIndex(x => x.EvtTipo);
        });
    }
}
