namespace Atracciones.Contracts.Events;

public static class EventTypes
{
    public const string ExchangeName = "atracciones.events";
    public const string DeadLetterExchange = "atracciones.dlx";
    public const string VirtualHost = "atracciones";

    public const string MarketplaceReservaSolicitada = "marketplace.reserva.solicitada";
    public const string MarketplaceReservaConfirmada = "marketplace.reserva.confirmada";
    public const string MarketplaceReservaRechazada = "marketplace.reserva.rechazada";
    public const string ReservasReservaPagada = "reservas.reserva.pagada";
    public const string AtraccionesHorarioCupoAgotado = "atracciones.horario.cupo_agotado";

    public const string QueueReservasMarketplace = "reservas.marketplace";
    public const string QueueAtraccionesMarketplaceSync = "atracciones.marketplace-sync";
    public const string QueueCrmMarketplaceActividad = "crm.marketplace-actividad";
    public const string QueueAuditMarketplace = "audit.marketplace";
    public const string QueueFacturacionReservasPagadas = "facturacion.reservas-pagadas";
    public const string DlqQueueName = "atracciones.dlq";
}
