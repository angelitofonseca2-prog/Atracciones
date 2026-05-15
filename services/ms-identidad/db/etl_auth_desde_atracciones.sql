-- ETL idempotente (Fase 1): copia usuarios/roles desde esquema atracciones hacia auth.*
-- Requisito: misma instancia PostgreSQL con ambos esquemas, o ejecutar por pasos export/import.
-- Verifica conteos antes/después.

BEGIN;

INSERT INTO auth.roles (rol_descripcion, rol_estado)
SELECT DISTINCT UPPER(TRIM(r.rol_descripcion)), 'A'
FROM atracciones.roles r
WHERE r.rol_estado = 'A'
ON CONFLICT (rol_descripcion) DO NOTHING;

INSERT INTO auth.usuarios (
    usu_id, usu_guid, usu_login, usu_password_hash,
    usu_fecha_registro, usu_usuario_registro, usu_ip_registro, usu_estado, cli_id
)
OVERRIDING SYSTEM VALUE
SELECT
    u.usu_id, u.usu_guid, u.usu_login, u.usu_password_hash,
    u.usu_fecha_registro, u.usu_usuario_registro, u.usu_ip_registro, u.usu_estado,
    c.cli_id
FROM atracciones.usuario u
LEFT JOIN atracciones.clientes c ON c.usu_id = u.usu_id AND c.cli_estado = 'A'
WHERE u.usu_estado = 'A'
ON CONFLICT (usu_id) DO UPDATE SET
    usu_guid = EXCLUDED.usu_guid,
    usu_login = EXCLUDED.usu_login,
    usu_password_hash = EXCLUDED.usu_password_hash,
    usu_estado = EXCLUDED.usu_estado,
    cli_id = EXCLUDED.cli_id;

INSERT INTO auth.usuario_roles (usu_id, rol_id, usu_rol_estado)
SELECT ur.usu_id, ar.rol_id, ur.usu_rol_estado
FROM atracciones.usuarioxroles ur
JOIN atracciones.roles r ON r.rol_id = ur.rol_id AND r.rol_estado = 'A'
JOIN auth.roles ar ON UPPER(ar.rol_descripcion) = UPPER(TRIM(r.rol_descripcion))
WHERE ur.usu_rol_estado = 'A'
ON CONFLICT (usu_id, rol_id) DO UPDATE SET usu_rol_estado = EXCLUDED.usu_rol_estado;

COMMIT;
