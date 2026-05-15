using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Microservicio.Atracciones.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "atracciones");

            migrationBuilder.CreateTable(
                name: "auditoria_log",
                schema: "atracciones",
                columns: table => new
                {
                    log_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    log_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    log_tabla = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    log_operacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    log_registro_id = table.Column<int>(type: "integer", nullable: true),
                    log_registro_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    log_datos_anteriores = table.Column<string>(type: "text", nullable: true),
                    log_datos_nuevos = table.Column<string>(type: "text", nullable: true),
                    log_fecha_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    log_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    log_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    log_origen_canal = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria_log", x => x.log_id);
                });

            migrationBuilder.CreateTable(
                name: "categoria",
                schema: "atracciones",
                columns: table => new
                {
                    cat_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    cat_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    cat_parent_id = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_categoria", x => x.cat_id);
                    table.ForeignKey(
                        name: "fk_categoria_parent",
                        column: x => x.cat_parent_id,
                        principalSchema: "atracciones",
                        principalTable: "categoria",
                        principalColumn: "cat_id");
                });

            migrationBuilder.CreateTable(
                name: "destino",
                schema: "atracciones",
                columns: table => new
                {
                    des_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    des_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
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
                    table.PrimaryKey("PK_destino", x => x.des_id);
                });

            migrationBuilder.CreateTable(
                name: "idioma",
                schema: "atracciones",
                columns: table => new
                {
                    id_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
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
                    table.PrimaryKey("PK_idioma", x => x.id_id);
                });

            migrationBuilder.CreateTable(
                name: "imagen",
                schema: "atracciones",
                columns: table => new
                {
                    img_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    img_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
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
                    table.PrimaryKey("PK_imagen", x => x.img_id);
                });

            migrationBuilder.CreateTable(
                name: "incluye",
                schema: "atracciones",
                columns: table => new
                {
                    inc_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    inc_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    inc_descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    inc_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incluye", x => x.inc_id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "atracciones",
                columns: table => new
                {
                    rol_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    rol_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    rol_descripcion = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    rol_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    rol_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rol_ip_ingreso = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    rol_fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rol_usuario_eliminacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rol_ip_eliminacion = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    rol_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.rol_id);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                schema: "atracciones",
                columns: table => new
                {
                    usu_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    usu_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    usu_login = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    usu_password_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    usu_fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    usu_usuario_registro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    usu_ip_registro = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    usu_fecha_mod = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    usu_usuario_mod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    usu_ip_mod = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    usu_fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    usu_usuario_eliminacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    usu_ip_eliminacion = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    usu_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.usu_id);
                });

            migrationBuilder.CreateTable(
                name: "atraccion",
                schema: "atracciones",
                columns: table => new
                {
                    at_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    at_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    des_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_atraccion", x => x.at_id);
                    table.ForeignKey(
                        name: "fk_atraccion_destino",
                        column: x => x.des_id,
                        principalSchema: "atracciones",
                        principalTable: "destino",
                        principalColumn: "des_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clientes",
                schema: "atracciones",
                columns: table => new
                {
                    cli_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    cli_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    usu_id = table.Column<int>(type: "integer", nullable: true),
                    cli_tipo_identificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cli_numero_identificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cli_nombres = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cli_apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cli_razon_social = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    cli_correo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    cli_telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cli_direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    cli_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    cli_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cli_ip_ingreso = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    cli_fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cli_usuario_eliminacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cli_ip_eliminacion = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    cli_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.cli_id);
                    table.ForeignKey(
                        name: "fk_clientes_usuario",
                        column: x => x.usu_id,
                        principalSchema: "atracciones",
                        principalTable: "usuario",
                        principalColumn: "usu_id");
                });

            migrationBuilder.CreateTable(
                name: "usuarioxroles",
                schema: "atracciones",
                columns: table => new
                {
                    usu_rol_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    usu_id = table.Column<int>(type: "integer", nullable: false),
                    rol_id = table.Column<int>(type: "integer", nullable: false),
                    usu_rol_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarioxroles", x => x.usu_rol_id);
                    table.ForeignKey(
                        name: "fk_usuarioxroles_rol",
                        column: x => x.rol_id,
                        principalSchema: "atracciones",
                        principalTable: "roles",
                        principalColumn: "rol_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_usuarioxroles_usuario",
                        column: x => x.usu_id,
                        principalSchema: "atracciones",
                        principalTable: "usuario",
                        principalColumn: "usu_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "atraccion_incluye",
                schema: "atracciones",
                columns: table => new
                {
                    inc_id = table.Column<int>(type: "integer", nullable: false),
                    at_id = table.Column<int>(type: "integer", nullable: false),
                    ai_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    ai_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ai_fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ai_usuario_eliminacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ai_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atraccion_incluye", x => new { x.inc_id, x.at_id });
                    table.ForeignKey(
                        name: "fk_ai_atraccion",
                        column: x => x.at_id,
                        principalSchema: "atracciones",
                        principalTable: "atraccion",
                        principalColumn: "at_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ai_incluye",
                        column: x => x.inc_id,
                        principalSchema: "atracciones",
                        principalTable: "incluye",
                        principalColumn: "inc_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "categoria_atraccion",
                schema: "atracciones",
                columns: table => new
                {
                    cat_id = table.Column<int>(type: "integer", nullable: false),
                    at_id = table.Column<int>(type: "integer", nullable: false),
                    ca_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    ca_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ca_fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ca_usuario_eliminacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ca_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria_atraccion", x => new { x.cat_id, x.at_id });
                    table.ForeignKey(
                        name: "fk_ca_atraccion",
                        column: x => x.at_id,
                        principalSchema: "atracciones",
                        principalTable: "atraccion",
                        principalColumn: "at_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ca_categoria",
                        column: x => x.cat_id,
                        principalSchema: "atracciones",
                        principalTable: "categoria",
                        principalColumn: "cat_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "idioma_atraccion",
                schema: "atracciones",
                columns: table => new
                {
                    id_id = table.Column<int>(type: "integer", nullable: false),
                    at_id = table.Column<int>(type: "integer", nullable: false),
                    ia_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    ia_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ia_fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ia_usuario_eliminacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ia_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idioma_atraccion", x => new { x.id_id, x.at_id });
                    table.ForeignKey(
                        name: "fk_ia_atraccion",
                        column: x => x.at_id,
                        principalSchema: "atracciones",
                        principalTable: "atraccion",
                        principalColumn: "at_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ia_idioma",
                        column: x => x.id_id,
                        principalSchema: "atracciones",
                        principalTable: "idioma",
                        principalColumn: "id_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "imagen_atraccion",
                schema: "atracciones",
                columns: table => new
                {
                    img_id = table.Column<int>(type: "integer", nullable: false),
                    at_id = table.Column<int>(type: "integer", nullable: false),
                    ima_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    ima_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ima_fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ima_usuario_eliminacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ima_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_imagen_atraccion", x => new { x.img_id, x.at_id });
                    table.ForeignKey(
                        name: "fk_ima_atraccion",
                        column: x => x.at_id,
                        principalSchema: "atracciones",
                        principalTable: "atraccion",
                        principalColumn: "at_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ima_imagen",
                        column: x => x.img_id,
                        principalSchema: "atracciones",
                        principalTable: "imagen",
                        principalColumn: "img_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ticket",
                schema: "atracciones",
                columns: table => new
                {
                    tck_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    tck_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    at_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ticket", x => x.tck_id);
                    table.ForeignKey(
                        name: "fk_ticket_atraccion",
                        column: x => x.at_id,
                        principalSchema: "atracciones",
                        principalTable: "atraccion",
                        principalColumn: "at_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "horario",
                schema: "atracciones",
                columns: table => new
                {
                    hor_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    hor_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tck_id = table.Column<int>(type: "integer", nullable: false),
                    hor_fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    hor_hora_inicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    hor_hora_fin = table.Column<TimeOnly>(type: "time", nullable: true),
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
                    table.PrimaryKey("PK_horario", x => x.hor_id);
                    table.ForeignKey(
                        name: "fk_horario_ticket",
                        column: x => x.tck_id,
                        principalSchema: "atracciones",
                        principalTable: "ticket",
                        principalColumn: "tck_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reservas",
                schema: "atracciones",
                columns: table => new
                {
                    rev_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    rev_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    rev_codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cli_id = table.Column<int>(type: "integer", nullable: false),
                    hor_id = table.Column<int>(type: "integer", nullable: false),
                    rev_fecha_reserva_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    rev_subtotal = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    rev_valor_iva = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    rev_total = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    rev_origen_canal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    rev_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rev_ip_ingreso = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    rev_fecha_mod = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rev_usuario_mod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rev_ip_mod = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    rev_fecha_cancelacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rev_usuario_cancelacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rev_ip_cancelacion = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    rev_motivo_cancelacion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    rev_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservas", x => x.rev_id);
                    table.ForeignKey(
                        name: "fk_reservas_cliente",
                        column: x => x.cli_id,
                        principalSchema: "atracciones",
                        principalTable: "clientes",
                        principalColumn: "cli_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_reservas_horario",
                        column: x => x.hor_id,
                        principalSchema: "atracciones",
                        principalTable: "horario",
                        principalColumn: "hor_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "facturas",
                schema: "atracciones",
                columns: table => new
                {
                    fac_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    fac_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    rev_id = table.Column<int>(type: "integer", nullable: false),
                    fac_numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fac_fecha_emision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    fac_total = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    fac_observacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fac_origen_canal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    fac_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fac_ip_ingreso = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    fac_fecha_mod = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fac_usuario_mod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fac_ip_mod = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    fac_fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fac_usuario_eliminacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fac_ip_eliminacion = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    fac_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A'),
                    fac_motivo_inhabilitacion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facturas", x => x.fac_id);
                    table.ForeignKey(
                        name: "fk_facturas_reserva",
                        column: x => x.rev_id,
                        principalSchema: "atracciones",
                        principalTable: "reservas",
                        principalColumn: "rev_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resenia",
                schema: "atracciones",
                columns: table => new
                {
                    rsn_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    rsn_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    at_id = table.Column<int>(type: "integer", nullable: false),
                    rev_id = table.Column<int>(type: "integer", nullable: false),
                    rsn_comentario = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    rsn_rating = table.Column<short>(type: "smallint", nullable: false),
                    rsn_fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    rsn_usuario_creacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rsn_ip_creacion = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    rsn_fecha_mod = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rsn_usuario_mod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rsn_ip_mod = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    rsn_fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rsn_usuario_eliminacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rsn_ip_eliminacion = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    rsn_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resenia", x => x.rsn_id);
                    table.ForeignKey(
                        name: "fk_resenia_atraccion",
                        column: x => x.at_id,
                        principalSchema: "atracciones",
                        principalTable: "atraccion",
                        principalColumn: "at_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resenia_reserva",
                        column: x => x.rev_id,
                        principalSchema: "atracciones",
                        principalTable: "reservas",
                        principalColumn: "rev_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reserva_detalle",
                schema: "atracciones",
                columns: table => new
                {
                    rdet_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    rdet_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    rev_id = table.Column<int>(type: "integer", nullable: false),
                    tck_id = table.Column<int>(type: "integer", nullable: false),
                    rdet_cantidad = table.Column<int>(type: "integer", nullable: false),
                    rdet_precio_unit = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    rdet_subtotal = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    rdet_fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    rdet_usuario_ingreso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rdet_ip_ingreso = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    rdet_fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rdet_usuario_eliminacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rdet_ip_eliminacion = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    rdet_estado = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A')
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reserva_detalle", x => x.rdet_id);
                    table.ForeignKey(
                        name: "fk_rdet_reserva",
                        column: x => x.rev_id,
                        principalSchema: "atracciones",
                        principalTable: "reservas",
                        principalColumn: "rev_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_rdet_ticket",
                        column: x => x.tck_id,
                        principalSchema: "atracciones",
                        principalTable: "ticket",
                        principalColumn: "tck_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "datos_facturacion",
                schema: "atracciones",
                columns: table => new
                {
                    dfac_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    dfac_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    fac_id = table.Column<int>(type: "integer", nullable: false),
                    dfac_nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    dfac_apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    dfac_correo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    dfac_telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_datos_facturacion", x => x.dfac_id);
                    table.ForeignKey(
                        name: "fk_datos_facturacion_fac",
                        column: x => x.fac_id,
                        principalSchema: "atracciones",
                        principalTable: "facturas",
                        principalColumn: "fac_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_atraccion_des_id",
                schema: "atracciones",
                table: "atraccion",
                column: "des_id");

            migrationBuilder.CreateIndex(
                name: "uk_atraccion_guid",
                schema: "atracciones",
                table: "atraccion",
                column: "at_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_atraccion_incluye_at_id",
                schema: "atracciones",
                table: "atraccion_incluye",
                column: "at_id");

            migrationBuilder.CreateIndex(
                name: "uk_auditoria_log_guid",
                schema: "atracciones",
                table: "auditoria_log",
                column: "log_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categoria_cat_parent_id",
                schema: "atracciones",
                table: "categoria",
                column: "cat_parent_id");

            migrationBuilder.CreateIndex(
                name: "uk_categoria_guid",
                schema: "atracciones",
                table: "categoria",
                column: "cat_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categoria_atraccion_at_id",
                schema: "atracciones",
                table: "categoria_atraccion",
                column: "at_id");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_usu_id",
                schema: "atracciones",
                table: "clientes",
                column: "usu_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_clientes_guid",
                schema: "atracciones",
                table: "clientes",
                column: "cli_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_clientes_num_identificacion",
                schema: "atracciones",
                table: "clientes",
                column: "cli_numero_identificacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_datos_facturacion_fac",
                schema: "atracciones",
                table: "datos_facturacion",
                column: "fac_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_datos_facturacion_guid",
                schema: "atracciones",
                table: "datos_facturacion",
                column: "dfac_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_destino_guid",
                schema: "atracciones",
                table: "destino",
                column: "des_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_facturas_guid",
                schema: "atracciones",
                table: "facturas",
                column: "fac_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_facturas_numero",
                schema: "atracciones",
                table: "facturas",
                column: "fac_numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_facturas_reserva",
                schema: "atracciones",
                table: "facturas",
                column: "rev_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_horario_guid",
                schema: "atracciones",
                table: "horario",
                column: "hor_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_horario_slot",
                schema: "atracciones",
                table: "horario",
                columns: new[] { "tck_id", "hor_fecha", "hor_hora_inicio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_idioma_descripcion",
                schema: "atracciones",
                table: "idioma",
                column: "id_descripcion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_idioma_guid",
                schema: "atracciones",
                table: "idioma",
                column: "id_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_idioma_atraccion_at_id",
                schema: "atracciones",
                table: "idioma_atraccion",
                column: "at_id");

            migrationBuilder.CreateIndex(
                name: "uk_imagen_guid",
                schema: "atracciones",
                table: "imagen",
                column: "img_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_imagen_atraccion_at_id",
                schema: "atracciones",
                table: "imagen_atraccion",
                column: "at_id");

            migrationBuilder.CreateIndex(
                name: "uk_incluye_guid",
                schema: "atracciones",
                table: "incluye",
                column: "inc_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_resenia_at_id",
                schema: "atracciones",
                table: "resenia",
                column: "at_id");

            migrationBuilder.CreateIndex(
                name: "uk_resenia_guid",
                schema: "atracciones",
                table: "resenia",
                column: "rsn_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_resenia_reserva",
                schema: "atracciones",
                table: "resenia",
                column: "rev_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reserva_detalle_tck_id",
                schema: "atracciones",
                table: "reserva_detalle",
                column: "tck_id");

            migrationBuilder.CreateIndex(
                name: "uk_rdet_guid",
                schema: "atracciones",
                table: "reserva_detalle",
                column: "rdet_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_rdet_rev_tck",
                schema: "atracciones",
                table: "reserva_detalle",
                columns: new[] { "rev_id", "tck_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservas_cli_id",
                schema: "atracciones",
                table: "reservas",
                column: "cli_id");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_hor_id",
                schema: "atracciones",
                table: "reservas",
                column: "hor_id");

            migrationBuilder.CreateIndex(
                name: "uk_reservas_codigo",
                schema: "atracciones",
                table: "reservas",
                column: "rev_codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_reservas_guid",
                schema: "atracciones",
                table: "reservas",
                column: "rev_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_roles_guid",
                schema: "atracciones",
                table: "roles",
                column: "rol_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ticket_at_id",
                schema: "atracciones",
                table: "ticket",
                column: "at_id");

            migrationBuilder.CreateIndex(
                name: "uk_ticket_guid",
                schema: "atracciones",
                table: "ticket",
                column: "tck_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_usuario_guid",
                schema: "atracciones",
                table: "usuario",
                column: "usu_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_usuario_login",
                schema: "atracciones",
                table: "usuario",
                column: "usu_login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarioxroles_rol_id",
                schema: "atracciones",
                table: "usuarioxroles",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "uk_usuarioxroles_par",
                schema: "atracciones",
                table: "usuarioxroles",
                columns: new[] { "usu_id", "rol_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "atraccion_incluye",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "auditoria_log",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "categoria_atraccion",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "datos_facturacion",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "idioma_atraccion",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "imagen_atraccion",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "resenia",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "reserva_detalle",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "usuarioxroles",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "incluye",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "categoria",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "facturas",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "idioma",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "imagen",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "reservas",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "clientes",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "horario",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "usuario",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "ticket",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "atraccion",
                schema: "atracciones");

            migrationBuilder.DropTable(
                name: "destino",
                schema: "atracciones");
        }
    }
}
