using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atracciones.MsAuditoria.DataAccess.Migrations;

public partial class AddEventBusAudit : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "eventos_procesados",
            schema: "audit",
            columns: table => new
            {
                event_id = table.Column<Guid>(type: "uuid", nullable: false),
                event_type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                processed_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_eventos_procesados", x => x.event_id));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "eventos_procesados", schema: "audit");
    }
}
