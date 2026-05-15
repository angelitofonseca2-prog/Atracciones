using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Atracciones.MsOrquestador.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialOrquestador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "orq");

            migrationBuilder.CreateTable(
                name: "idempotency_keys",
                schema: "orq",
                columns: table => new
                {
                    storage_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    response_json = table.Column<string>(type: "text", nullable: false),
                    created_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotency_keys", x => x.storage_key);
                });

            migrationBuilder.CreateTable(
                name: "saga_state",
                schema: "orq",
                columns: table => new
                {
                    saga_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    estado = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    inicio_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fin_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saga_state", x => x.saga_id);
                });

            migrationBuilder.CreateTable(
                name: "saga_pasos",
                schema: "orq",
                columns: table => new
                {
                    paso_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    saga_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paso = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    estado = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    request_payload = table.Column<string>(type: "text", nullable: true),
                    response_payload = table.Column<string>(type: "text", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true),
                    fecha_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saga_pasos", x => x.paso_id);
                    table.ForeignKey(
                        name: "FK_saga_pasos_saga_state_saga_id",
                        column: x => x.saga_id,
                        principalSchema: "orq",
                        principalTable: "saga_state",
                        principalColumn: "saga_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_saga_pasos_saga_id",
                schema: "orq",
                table: "saga_pasos",
                column: "saga_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "idempotency_keys",
                schema: "orq");

            migrationBuilder.DropTable(
                name: "saga_pasos",
                schema: "orq");

            migrationBuilder.DropTable(
                name: "saga_state",
                schema: "orq");
        }
    }
}
