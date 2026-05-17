using Atracciones.MsOrquestador.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsOrquestador.DataAccess.Context;

public sealed class OrquestadorDbContext : DbContext
{
    public OrquestadorDbContext(DbContextOptions<OrquestadorDbContext> options)
        : base(options)
    {
    }

    public DbSet<SagaStateEntity> SagaStates => Set<SagaStateEntity>();
    public DbSet<SagaPasoEntity> SagaPasos => Set<SagaPasoEntity>();
    public DbSet<IdempotencyKeyEntity> IdempotencyKeys => Set<IdempotencyKeyEntity>();
    public DbSet<PayPalPaymentEntity> PayPalPayments => Set<PayPalPaymentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("orq");

        modelBuilder.Entity<SagaStateEntity>(b =>
        {
            b.ToTable("saga_state");
            b.HasKey(x => x.SagaId);
            b.Property(x => x.SagaId).HasColumnName("saga_id");
            b.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(64).IsRequired();
            b.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(32).IsRequired();
            b.Property(x => x.CorrelationId).HasColumnName("correlation_id").HasMaxLength(64).IsRequired();
            b.Property(x => x.InicioUtc).HasColumnName("inicio_utc");
            b.Property(x => x.FinUtc).HasColumnName("fin_utc");
        });

        modelBuilder.Entity<SagaPasoEntity>(b =>
        {
            b.ToTable("saga_pasos");
            b.HasKey(x => x.PasoId);
            b.Property(x => x.PasoId).HasColumnName("paso_id").UseIdentityByDefaultColumn();
            b.Property(x => x.SagaId).HasColumnName("saga_id");
            b.Property(x => x.Paso).HasColumnName("paso").HasMaxLength(128).IsRequired();
            b.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(32).IsRequired();
            b.Property(x => x.RequestPayload).HasColumnName("request_payload");
            b.Property(x => x.ResponsePayload).HasColumnName("response_payload");
            b.Property(x => x.Error).HasColumnName("error");
            b.Property(x => x.FechaUtc).HasColumnName("fecha_utc");
            b.HasOne(x => x.Saga)
                .WithMany(x => x.Pasos)
                .HasForeignKey(x => x.SagaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IdempotencyKeyEntity>(b =>
        {
            b.ToTable("idempotency_keys");
            b.HasKey(x => x.StorageKey);
            b.Property(x => x.StorageKey).HasColumnName("storage_key").HasMaxLength(512);
            b.Property(x => x.ResponseJson).HasColumnName("response_json").IsRequired();
            b.Property(x => x.CreatedUtc).HasColumnName("created_utc");
        });

        modelBuilder.Entity<PayPalPaymentEntity>(b =>
        {
            b.ToTable("paypal_payments");
            b.HasKey(x => x.PayPaymentId);
            b.Property(x => x.PayPaymentId).HasColumnName("pay_payment_id").UseIdentityByDefaultColumn();
            b.Property(x => x.RevGuid).HasColumnName("rev_guid");
            b.Property(x => x.PaypalOrderId).HasColumnName("paypal_order_id").HasMaxLength(64).IsRequired();
            b.Property(x => x.PaypalCaptureId).HasColumnName("paypal_capture_id").HasMaxLength(64);
            b.Property(x => x.EstadoPago).HasColumnName("estado_pago").HasMaxLength(32).IsRequired();
            b.Property(x => x.MontoEsperado).HasColumnName("monto_esperado").HasPrecision(18, 2);
            b.Property(x => x.Moneda).HasColumnName("moneda").HasMaxLength(8).IsRequired();
            b.Property(x => x.ChargebackStatus).HasColumnName("chargeback_status").HasMaxLength(64);
            b.Property(x => x.CheckoutPayloadJson).HasColumnName("checkout_payload_json");
            b.Property(x => x.CreatedUtc).HasColumnName("created_utc");
            b.Property(x => x.UpdatedUtc).HasColumnName("updated_utc");
            b.HasIndex(x => x.PaypalOrderId).IsUnique();
            b.HasIndex(x => x.PaypalCaptureId).IsUnique();
        });
    }
}
