using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atracciones.MsAtracciones.DataAccess.Migrations.Catalogos
{
    /// <inheritdoc />
    public partial class InitialCatalogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalogos");

            migrationBuilder.CreateTable(
                name: "categorias",
                schema: "catalogos",
                columns: table => new
                {
                    cat_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    cat_parent_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    cat_nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cat_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    cat_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cat_ip_ingreso = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    cat_fecha_mod = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cat_usuario_mod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cat_ip_mod = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    cat_fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cat_usuario_eliminacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cat_ip_eliminacion = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    cat_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.cat_guid);
                    table.ForeignKey(
                        name: "FK_categorias_categorias_cat_parent_guid",
                        column: x => x.cat_parent_guid,
                        principalSchema: "catalogos",
                        principalTable: "categorias",
                        principalColumn: "cat_guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "destinos",
                schema: "catalogos",
                columns: table => new
                {
                    des_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    des_nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    des_pais = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    des_imagen_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    des_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    des_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    des_ip_ingreso = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    des_fecha_mod = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    des_usuario_mod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    des_ip_mod = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    des_fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    des_usuario_eliminacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    des_ip_eliminacion = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    des_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_destinos", x => x.des_guid);
                });

            migrationBuilder.CreateTable(
                name: "idiomas",
                schema: "catalogos",
                columns: table => new
                {
                    id_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    id_descripcion = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    id_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    id_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    id_ip_ingreso = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    id_fecha_mod = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_mod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    id_ip_mod = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    id_fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_eliminacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    id_ip_eliminacion = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    id_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idiomas", x => x.id_guid);
                });

            migrationBuilder.CreateTable(
                name: "imagenes",
                schema: "catalogos",
                columns: table => new
                {
                    img_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    img_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    img_descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    img_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    img_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    img_ip_ingreso = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    img_fecha_mod = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    img_usuario_mod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    img_ip_mod = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    img_fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    img_usuario_eliminacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    img_ip_eliminacion = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    img_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_imagenes", x => x.img_guid);
                });

            migrationBuilder.CreateTable(
                name: "incluye",
                schema: "catalogos",
                columns: table => new
                {
                    inc_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    inc_descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    inc_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incluye", x => x.inc_guid);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categorias_cat_parent_guid",
                schema: "catalogos",
                table: "categorias",
                column: "cat_parent_guid");

            migrationBuilder.CreateIndex(
                name: "uk_idiomas_descripcion",
                schema: "catalogos",
                table: "idiomas",
                column: "id_descripcion",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "categorias",
                schema: "catalogos");

            migrationBuilder.DropTable(
                name: "destinos",
                schema: "catalogos");

            migrationBuilder.DropTable(
                name: "idiomas",
                schema: "catalogos");

            migrationBuilder.DropTable(
                name: "imagenes",
                schema: "catalogos");

            migrationBuilder.DropTable(
                name: "incluye",
                schema: "catalogos");
        }
    }
}
