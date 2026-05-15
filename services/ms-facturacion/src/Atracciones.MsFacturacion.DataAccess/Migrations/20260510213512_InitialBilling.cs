using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atracciones.MsFacturacion.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "billing");

            migrationBuilder.CreateTable(
                name: "facturas",
                schema: "billing",
                columns: table => new
                {
                    fac_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    rev_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    cli_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    fac_numero = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    fac_total = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    fac_moneda = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false, defaultValue: "USD"),
                    fac_fecha_emision_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fac_estado = table.Column<char>(type: "char(1)", fixedLength: true, maxLength: 1, nullable: false),
                    rev_codigo_snap = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    fac_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fac_ip_ingreso = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facturas", x => x.fac_guid);
                });

            migrationBuilder.CreateTable(
                name: "datos_facturacion",
                schema: "billing",
                columns: table => new
                {
                    dfac_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    fac_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    dfac_nombre = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    dfac_correo = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    dfac_telefono = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_datos_facturacion", x => x.dfac_guid);
                    table.ForeignKey(
                        name: "FK_datos_facturacion_facturas_fac_guid",
                        column: x => x.fac_guid,
                        principalSchema: "billing",
                        principalTable: "facturas",
                        principalColumn: "fac_guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_datos_facturacion_fac_guid",
                schema: "billing",
                table: "datos_facturacion",
                column: "fac_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_facturas_cli_guid",
                schema: "billing",
                table: "facturas",
                column: "cli_guid");

            migrationBuilder.CreateIndex(
                name: "IX_facturas_fac_numero",
                schema: "billing",
                table: "facturas",
                column: "fac_numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_facturas_rev_guid",
                schema: "billing",
                table: "facturas",
                column: "rev_guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "datos_facturacion",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "facturas",
                schema: "billing");
        }
    }
}
