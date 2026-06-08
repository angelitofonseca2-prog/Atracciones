using Atracciones.MsReservas.DataAccess.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atracciones.MsReservas.DataAccess.Migrations;

[DbContext(typeof(VentasDbContext))]
[Migration("20260607120000_AddEventBusVentas")]
public partial class AddEventBusVentas : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "eventos_procesados",
            schema: "ventas",
            columns: table => new
            {
                event_id = table.Column<Guid>(type: "uuid", nullable: false),
                event_type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                processed_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_eventos_procesados", x => x.event_id));

        migrationBuilder.CreateTable(
            name: "marketplace_reserva_seguimiento",
            schema: "ventas",
            columns: table => new
            {
                seguimiento_id = table.Column<Guid>(type: "uuid", nullable: false),
                rev_guid = table.Column<Guid>(type: "uuid", nullable: true),
                estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                rev_codigo = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                motivo_rechazo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                correlation_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                created_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_marketplace_reserva_seguimiento", x => x.seguimiento_id));

        migrationBuilder.CreateTable(
            name: "outbox_events",
            schema: "ventas",
            columns: table => new
            {
                ob_guid = table.Column<Guid>(type: "uuid", nullable: false),
                routing_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                payload_json = table.Column<string>(type: "text", nullable: false),
                correlation_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                created_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                published_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_outbox_events", x => x.ob_guid));

        migrationBuilder.CreateIndex(
            name: "IX_marketplace_reserva_seguimiento_correlation_id",
            schema: "ventas",
            table: "marketplace_reserva_seguimiento",
            column: "correlation_id");

        migrationBuilder.CreateIndex(
            name: "IX_outbox_events_published_utc",
            schema: "ventas",
            table: "outbox_events",
            column: "published_utc");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "eventos_procesados", schema: "ventas");
        migrationBuilder.DropTable(name: "marketplace_reserva_seguimiento", schema: "ventas");
        migrationBuilder.DropTable(name: "outbox_events", schema: "ventas");
    }
}
