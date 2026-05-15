using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atracciones.MsReservas.DataAccess.Migrations.Crm
{
    /// <inheritdoc />
    public partial class InitialCrm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "crm");

            migrationBuilder.CreateTable(
                name: "clientes",
                schema: "crm",
                columns: table => new
                {
                    cli_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    cli_tipo_identificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cli_numero_identificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cli_nombres = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cli_apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cli_razon_social = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    cli_correo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    cli_telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cli_direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    cli_estado = table.Column<char>(type: "char(1)", fixedLength: true, maxLength: 1, nullable: false, defaultValue: 'A'),
                    cli_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    cli_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cli_ip_ingreso = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.cli_guid);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clientes_cli_numero_identificacion",
                schema: "crm",
                table: "clientes",
                column: "cli_numero_identificacion",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clientes",
                schema: "crm");
        }
    }
}
