-- Admin manual en Railway (alternativa a variables BootstrapAdmin__* en ms-identidad).
-- La contraseña en BD debe ser hash BCrypt, NO texto plano (si no, login = "Credenciales inválidas").
--
-- Credenciales:
--   Login:    admin@atracciones.local
--   Password: AdminAtracciones2026!
--
-- Tras ejecutar: cerrar sesión en el frontend y volver a iniciar sesión.

INSERT INTO auth.usuarios (
    usu_login, usu_password_hash, usu_usuario_registro, usu_ip_registro, usu_estado)
SELECT
    'admin@atracciones.local',
    '$2a$12$YWXTmwp9XSravjGf/u1UZ.rk3R8DPn9qAsBKMH2f6w325.igOl3K6',
    'sql-bootstrap',
    '127.0.0.1',
    'A'
WHERE NOT EXISTS (
    SELECT 1 FROM auth.usuarios WHERE usu_login = 'admin@atracciones.local');

UPDATE auth.usuarios
SET usu_password_hash = '$2a$12$YWXTmwp9XSravjGf/u1UZ.rk3R8DPn9qAsBKMH2f6w325.igOl3K6',
    usu_estado = 'A'
WHERE usu_login = 'admin@atracciones.local';

INSERT INTO auth.usuario_roles (usu_id, rol_id, usu_rol_estado)
SELECT u.usu_id, r.rol_id, 'A'
FROM auth.usuarios u
CROSS JOIN auth.roles r
WHERE u.usu_login = 'admin@atracciones.local'
  AND r.rol_descripcion = 'ADMIN'
ON CONFLICT (usu_id, rol_id) DO UPDATE SET usu_rol_estado = 'A';

SELECT u.usu_login, r.rol_descripcion, LEFT(u.usu_password_hash, 7) AS hash_prefix
FROM auth.usuarios u
LEFT JOIN auth.usuario_roles ur ON ur.usu_id = u.usu_id AND ur.usu_rol_estado = 'A'
LEFT JOIN auth.roles r ON r.rol_id = ur.rol_id
WHERE u.usu_login = 'admin@atracciones.local';
