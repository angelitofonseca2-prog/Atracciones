using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atracciones.MsReservas.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialVentas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ventas");

            migrationBuilder.CreateTable(
                name: "reservas",
                schema: "ventas",
                columns: table => new
                {
                    rev_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    cli_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    at_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    hor_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    rev_codigo = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    rev_estado = table.Column<char>(type: "char(1)", fixedLength: true, maxLength: 1, nullable: false),
                    rev_subtotal = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    rev_valor_iva = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    rev_total = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    rev_moneda = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false, defaultValue: "USD"),
                    rev_origen_canal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    rev_fecha_reserva_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rev_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rev_ip_ingreso = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    atraccion_nombre_snap = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    hor_fecha_snap = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    hor_hora_inicio_snap = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    hor_hora_fin_snap = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservas", x => x.rev_guid);
                });

            migrationBuilder.CreateTable(
                name: "reserva_detalle",
                schema: "ventas",
                columns: table => new
                {
                    rdet_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    rev_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    tck_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio_unit = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    subtotal_linea = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    tipo_participante = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reserva_detalle", x => x.rdet_guid);
                    table.ForeignKey(
                        name: "FK_reserva_detalle_reservas_rev_guid",
                        column: x => x.rev_guid,
                        principalSchema: "ventas",
                        principalTable: "reservas",
                        principalColumn: "rev_guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reserva_detalle_rev_guid",
                schema: "ventas",
                table: "reserva_detalle",
                column: "rev_guid");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_cli_guid",
                schema: "ventas",
                table: "reservas",
                column: "cli_guid");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_rev_codigo",
                schema: "ventas",
                table: "reservas",
                column: "rev_codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reserva_detalle",
                schema: "ventas");

            migrationBuilder.DropTable(
                name: "reservas",
                schema: "ventas");
        }
    }
}
