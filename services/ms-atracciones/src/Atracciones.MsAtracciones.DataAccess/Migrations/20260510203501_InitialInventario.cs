using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atracciones.MsAtracciones.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inventario");

            migrationBuilder.CreateTable(
                name: "atracciones",
                schema: "inventario",
                columns: table => new
                {
                    at_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    des_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    des_nombre_snap = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    des_pais_snap = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    at_num_establecimiento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    at_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    at_descripcion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    at_total_resenias = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    at_direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    at_duracion_minutos = table.Column<int>(type: "integer", nullable: true),
                    at_punto_encuentro = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    at_precio_referencia = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    at_incluye_acompaniante = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    at_incluye_transporte = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    at_disponible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    at_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    at_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    at_ip_ingreso = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    at_fecha_mod = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    at_usuario_mod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    at_ip_mod = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    at_fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    at_usuario_eliminacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    at_ip_eliminacion = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    at_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atracciones", x => x.at_guid);
                });

            migrationBuilder.CreateTable(
                name: "atraccion_categoria",
                schema: "inventario",
                columns: table => new
                {
                    at_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    cat_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    ca_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A'),
                    ca_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    ca_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atraccion_categoria", x => new { x.at_guid, x.cat_guid });
                    table.ForeignKey(
                        name: "FK_atraccion_categoria_atracciones_at_guid",
                        column: x => x.at_guid,
                        principalSchema: "inventario",
                        principalTable: "atracciones",
                        principalColumn: "at_guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "atraccion_idioma",
                schema: "inventario",
                columns: table => new
                {
                    AtGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    IdGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    id_descripcion_snap = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ia_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A'),
                    ia_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    ia_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atraccion_idioma", x => new { x.AtGuid, x.IdGuid });
                    table.ForeignKey(
                        name: "FK_atraccion_idioma_atracciones_AtGuid",
                        column: x => x.AtGuid,
                        principalSchema: "inventario",
                        principalTable: "atracciones",
                        principalColumn: "at_guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "atraccion_imagen",
                schema: "inventario",
                columns: table => new
                {
                    AtGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ImgGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    img_url_snap = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ima_orden = table.Column<int>(type: "integer", nullable: false),
                    ima_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A'),
                    ima_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    ima_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atraccion_imagen", x => new { x.AtGuid, x.ImgGuid });
                    table.ForeignKey(
                        name: "FK_atraccion_imagen_atracciones_AtGuid",
                        column: x => x.AtGuid,
                        principalSchema: "inventario",
                        principalTable: "atracciones",
                        principalColumn: "at_guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "atraccion_incluye",
                schema: "inventario",
                columns: table => new
                {
                    AtGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    IncGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    inc_descripcion_snap = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ai_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A'),
                    ai_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    ai_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atraccion_incluye", x => new { x.AtGuid, x.IncGuid });
                    table.ForeignKey(
                        name: "FK_atraccion_incluye_atracciones_AtGuid",
                        column: x => x.AtGuid,
                        principalSchema: "inventario",
                        principalTable: "atracciones",
                        principalColumn: "at_guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resenias",
                schema: "inventario",
                columns: table => new
                {
                    rsn_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    at_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    rev_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    rsn_comentario = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    rsn_rating = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    rsn_fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    rsn_usuario_creacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rsn_ip_creacion = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    RsnFechaMod = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RsnUsuarioMod = table.Column<string>(type: "text", nullable: true),
                    RsnIpMod = table.Column<string>(type: "text", nullable: true),
                    RsnFechaEliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RsnUsuarioEliminacion = table.Column<string>(type: "text", nullable: true),
                    RsnIpEliminacion = table.Column<string>(type: "text", nullable: true),
                    rsn_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resenias", x => x.rsn_guid);
                    table.ForeignKey(
                        name: "FK_resenias_atracciones_at_guid",
                        column: x => x.at_guid,
                        principalSchema: "inventario",
                        principalTable: "atracciones",
                        principalColumn: "at_guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tickets",
                schema: "inventario",
                columns: table => new
                {
                    tck_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    at_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    tck_titulo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    tck_precio = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    tck_tipo_participante = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Adulto"),
                    tck_capacidad_maxima = table.Column<int>(type: "integer", nullable: false),
                    tck_cupos_disponibles = table.Column<int>(type: "integer", nullable: false),
                    tck_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    tck_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tck_ip_ingreso = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    tck_fecha_mod = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tck_usuario_mod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tck_ip_mod = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    tck_fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tck_usuario_eliminacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tck_ip_eliminacion = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    tck_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tickets", x => x.tck_guid);
                    table.ForeignKey(
                        name: "FK_tickets_atracciones_at_guid",
                        column: x => x.at_guid,
                        principalSchema: "inventario",
                        principalTable: "atracciones",
                        principalColumn: "at_guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "horarios",
                schema: "inventario",
                columns: table => new
                {
                    hor_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    tck_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    hor_fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    hor_hora_inicio = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    hor_hora_fin = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    hor_cupos_disponibles = table.Column<int>(type: "integer", nullable: false),
                    hor_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    hor_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    hor_ip_ingreso = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    hor_fecha_mod = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    hor_usuario_mod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    hor_ip_mod = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    hor_fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    hor_usuario_eliminacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    hor_ip_eliminacion = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    hor_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_horarios", x => x.hor_guid);
                    table.ForeignKey(
                        name: "FK_horarios_tickets_tck_guid",
                        column: x => x.tck_guid,
                        principalSchema: "inventario",
                        principalTable: "tickets",
                        principalColumn: "tck_guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "uk_horario_slot",
                schema: "inventario",
                table: "horarios",
                columns: new[] { "tck_guid", "hor_fecha", "hor_hora_inicio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_resenias_at_guid",
                schema: "inventario",
                table: "resenias",
                column: "at_guid");

            migrationBuilder.CreateIndex(
                name: "uk_resenia_rev_guid",
                schema: "inventario",
                table: "resenias",
                column: "rev_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tickets_at_guid",
                schema: "inventario",
                table: "tickets",
                column: "at_guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "atraccion_categoria",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "atraccion_idioma",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "atraccion_imagen",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "atraccion_incluye",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "horarios",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "resenias",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "tickets",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "atracciones",
                schema: "inventario");
        }
    }
}
