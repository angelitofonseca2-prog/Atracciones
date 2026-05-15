using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atracciones.MsAuditoria.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.CreateTable(
                name: "eventos",
                schema: "audit",
                columns: table => new
                {
                    evt_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    evt_tipo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    payload_json = table.Column<string>(type: "text", nullable: false),
                    fecha_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos", x => x.evt_guid);
                });

            migrationBuilder.CreateIndex(
                name: "IX_eventos_correlation_id",
                schema: "audit",
                table: "eventos",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "IX_eventos_evt_tipo",
                schema: "audit",
                table: "eventos",
                column: "evt_tipo");

            migrationBuilder.CreateIndex(
                name: "IX_eventos_fecha_utc",
                schema: "audit",
                table: "eventos",
                column: "fecha_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "eventos",
                schema: "audit");
        }
    }
}
