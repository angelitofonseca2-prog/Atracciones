using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atracciones.MsAtracciones.DataAccess.Migrations;

public partial class AddEventBusInventario : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "eventos_procesados",
            schema: "inventario",
            columns: table => new
            {
                event_id = table.Column<Guid>(type: "uuid", nullable: false),
                event_type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                processed_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_eventos_procesados", x => x.event_id));

        migrationBuilder.CreateTable(
            name: "outbox_events",
            schema: "inventario",
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
            name: "IX_outbox_events_published_utc",
            schema: "inventario",
            table: "outbox_events",
            column: "published_utc");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "eventos_procesados", schema: "inventario");
        migrationBuilder.DropTable(name: "outbox_events", schema: "inventario");
    }
}
