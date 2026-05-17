using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atracciones.MsOrquestador.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PayPalCheckoutPayload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "checkout_payload_json",
                schema: "orq",
                table: "paypal_payments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "checkout_payload_json",
                schema: "orq",
                table: "paypal_payments");
        }
    }
}
