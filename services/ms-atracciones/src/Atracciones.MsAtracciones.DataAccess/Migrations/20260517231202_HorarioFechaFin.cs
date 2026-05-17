using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atracciones.MsAtracciones.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class HorarioFechaFin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "hor_fecha_fin",
                schema: "inventario",
                table: "horarios",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "hor_fecha_fin",
                schema: "inventario",
                table: "horarios");
        }
    }
}
