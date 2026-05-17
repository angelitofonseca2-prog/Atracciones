-- =============================================================================
-- Diagnóstico paso a paso en Railway → Postgres → Database → Data → Query
-- Ejecuta cada bloque por separado (o todo el archivo) y lee el resultado.
-- =============================================================================

-- PASO 1: ¿En qué base estás conectado?
SELECT current_database() AS base_actual, current_user AS usuario_db;

-- PASO 2: ¿Existe el esquema auth? (si vacío → ms-identidad no migró en ESTA base)
SELECT schema_name
FROM information_schema.schemata
WHERE schema_name = 'auth';

-- PASO 3: ¿Existen tablas de identidad?
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'auth'
ORDER BY table_name;

-- PASO 4: ¿Hay usuarios? (si total = 0, el SELECT por correo devolverá "0 rows" — es normal)
SELECT COUNT(*) AS total_usuarios FROM auth.usuarios;

-- PASO 5: Listar TODOS los logins (así ves el correo exacto guardado al registrarte)
SELECT usu_id, usu_login, usu_estado, LEFT(usu_password_hash, 7) AS debe_ser_$2a$12
FROM auth.usuarios
ORDER BY usu_id;

-- PASO 6: Roles disponibles (debe existir ADMIN y CLIENTE)
SELECT rol_id, rol_descripcion, rol_estado FROM auth.roles ORDER BY rol_id;

-- PASO 7: Usuarios con sus roles
SELECT u.usu_login, r.rol_descripcion
FROM auth.usuarios u
LEFT JOIN auth.usuario_roles ur ON ur.usu_id = u.usu_id AND ur.usu_rol_estado = 'A'
LEFT JOIN auth.roles r ON r.rol_id = ur.rol_id
ORDER BY u.usu_login;

-- =============================================================================
-- Si PASO 2 no devuelve "auth" → redeploy ms-identidad con DATABASE_URL a esta Postgres.
-- Si PASO 4 = 0 → ejecuta bootstrap_admin.sql (crea admin) o regístrate de nuevo en la web.
-- Si PASO 5 muestra ines@gmail.com pero hash NO empieza por $2a$12 → contraseña mal guardada.
-- =============================================================================
