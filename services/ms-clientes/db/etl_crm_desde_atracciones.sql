-- ETL idempotente (Fase 2): copia perfiles de cliente desde atracciones.clientes hacia crm.clientes.
-- La PK en CRM es cli_guid = usu_guid del usuario vinculado (misma convención que el mirror HTTP).
-- Requisito: misma instancia PostgreSQL con esquemas atracciones y crm, o export/import por pasos.

BEGIN;

INSERT INTO crm.clientes (
    cli_guid,
    cli_tipo_identificacion,
    cli_numero_identificacion,
    cli_nombres,
    cli_apellidos,
    cli_razon_social,
    cli_correo,
    cli_telefono,
    cli_direccion,
    cli_estado,
    cli_fecha_ingreso,
    cli_usuario_ingreso,
    cli_ip_ingreso
)
SELECT
    u.usu_guid,
    c.cli_tipo_identificacion,
    c.cli_numero_identificacion,
    c.cli_nombres,
    c.cli_apellidos,
    c.cli_razon_social,
    c.cli_correo,
    c.cli_telefono,
    c.cli_direccion,
    c.cli_estado,
    c.cli_fecha_ingreso,
    c.cli_usuario_ingreso,
    c.cli_ip_ingreso
FROM atracciones.clientes c
INNER JOIN atracciones.usuario u ON u.usu_id = c.usu_id
WHERE c.cli_estado = 'A'
  AND c.usu_id IS NOT NULL
ON CONFLICT (cli_guid) DO UPDATE SET
    cli_tipo_identificacion = EXCLUDED.cli_tipo_identificacion,
    cli_numero_identificacion = EXCLUDED.cli_numero_identificacion,
    cli_nombres = EXCLUDED.cli_nombres,
    cli_apellidos = EXCLUDED.cli_apellidos,
    cli_razon_social = EXCLUDED.cli_razon_social,
    cli_correo = EXCLUDED.cli_correo,
    cli_telefono = EXCLUDED.cli_telefono,
    cli_direccion = EXCLUDED.cli_direccion,
    cli_estado = EXCLUDED.cli_estado;

COMMIT;
