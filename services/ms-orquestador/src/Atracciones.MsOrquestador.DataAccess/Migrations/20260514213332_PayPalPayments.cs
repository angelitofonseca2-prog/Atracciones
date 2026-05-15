using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Atracciones.MsOrquestador.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PayPalPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "paypal_payments",
                schema: "orq",
                columns: table => new
                {
                    pay_payment_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rev_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    paypal_order_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    paypal_capture_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    estado_pago = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    monto_esperado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    moneda = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    chargeback_status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paypal_payments", x => x.pay_payment_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_paypal_payments_paypal_capture_id",
                schema: "orq",
                table: "paypal_payments",
                column: "paypal_capture_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_paypal_payments_paypal_order_id",
                schema: "orq",
                table: "paypal_payments",
                column: "paypal_order_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "paypal_payments",
                schema: "orq");
        }
    }
}
