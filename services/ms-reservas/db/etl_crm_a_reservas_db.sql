-- ETL idempotente: migra datos del esquema crm (BD original de ms-clientes)
-- al esquema crm dentro de reservas_db.
--
-- Prerequisito: La migración EF de CrmDbContext ya creó el esquema crm
-- en reservas_db con la tabla clientes.
--
-- Uso ejemplo:
--   psql "host=localhost port=5437 dbname=reservas_db user=ventas password=ventas" \
--        -f etl_crm_a_reservas_db.sql
--
-- Ajustar src_conn si el host/puerto/credenciales de la BD origen difieren.

CREATE EXTENSION IF NOT EXISTS dblink;

DO $$
DECLARE
    src_conn TEXT := 'host=localhost port=5437 dbname=crm_db user=clientes password=clientes';
BEGIN

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
    cli_guid::uuid,
    cli_tipo_identificacion,
    cli_numero_identificacion,
    cli_nombres,
    cli_apellidos,
    cli_razon_social,
    cli_correo,
    cli_telefono,
    cli_direccion,
    cli_estado::char,
    cli_fecha_ingreso,
    cli_usuario_ingreso,
    cli_ip_ingreso
FROM dblink(src_conn,
    'SELECT cli_guid, cli_tipo_identificacion, cli_numero_identificacion,
            cli_nombres, cli_apellidos, cli_razon_social,
            cli_correo, cli_telefono, cli_direccion,
            cli_estado, cli_fecha_ingreso, cli_usuario_ingreso, cli_ip_ingreso
     FROM crm.clientes')
AS t(cli_guid text, cli_tipo_identificacion text, cli_numero_identificacion text,
     cli_nombres text, cli_apellidos text, cli_razon_social text,
     cli_correo text, cli_telefono text, cli_direccion text,
     cli_estado char, cli_fecha_ingreso timestamptz, cli_usuario_ingreso text, cli_ip_ingreso text)
ON CONFLICT (cli_guid) DO UPDATE SET
    cli_tipo_identificacion   = EXCLUDED.cli_tipo_identificacion,
    cli_numero_identificacion = EXCLUDED.cli_numero_identificacion,
    cli_nombres               = EXCLUDED.cli_nombres,
    cli_apellidos             = EXCLUDED.cli_apellidos,
    cli_razon_social          = EXCLUDED.cli_razon_social,
    cli_correo                = EXCLUDED.cli_correo,
    cli_telefono              = EXCLUDED.cli_telefono,
    cli_direccion             = EXCLUDED.cli_direccion,
    cli_estado                = EXCLUDED.cli_estado;

RAISE NOTICE 'ETL crm → reservas_db completado. Filas insertadas/actualizadas: %',
    (SELECT count(*) FROM crm.clientes);
END $$;
