using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Atracciones.MsIdentidad.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "auth",
                columns: table => new
                {
                    rol_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rol_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    rol_descripcion = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    rol_estado = table.Column<char>(type: "char(1)", fixedLength: true, maxLength: 1, nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.rol_id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                schema: "auth",
                columns: table => new
                {
                    usu_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usu_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    usu_login = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    usu_password_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    usu_fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    usu_usuario_registro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    usu_ip_registro = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    usu_estado = table.Column<char>(type: "char(1)", fixedLength: true, maxLength: 1, nullable: false, defaultValue: 'A'),
                    cli_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.usu_id);
                });

            migrationBuilder.CreateTable(
                name: "usuario_roles",
                schema: "auth",
                columns: table => new
                {
                    usu_id = table.Column<int>(type: "integer", nullable: false),
                    rol_id = table.Column<int>(type: "integer", nullable: false),
                    usu_rol_estado = table.Column<char>(type: "char(1)", fixedLength: true, maxLength: 1, nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_roles", x => new { x.usu_id, x.rol_id });
                    table.ForeignKey(
                        name: "FK_usuario_roles_roles_rol_id",
                        column: x => x.rol_id,
                        principalSchema: "auth",
                        principalTable: "roles",
                        principalColumn: "rol_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuario_roles_usuarios_usu_id",
                        column: x => x.usu_id,
                        principalSchema: "auth",
                        principalTable: "usuarios",
                        principalColumn: "usu_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_roles_rol_descripcion",
                schema: "auth",
                table: "roles",
                column: "rol_descripcion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_roles_rol_id",
                schema: "auth",
                table: "usuario_roles",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_usu_guid",
                schema: "auth",
                table: "usuarios",
                column: "usu_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_usu_login",
                schema: "auth",
                table: "usuarios",
                column: "usu_login",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usuario_roles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "usuarios",
                schema: "auth");
        }
    }
}
