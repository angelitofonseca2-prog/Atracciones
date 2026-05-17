-- =============================================================================
-- Pasar un usuario CLIENTE a ADMIN (ms-identidad / auth.*)
-- Sustituye 'TU_CORREO@ejemplo.com' por el login exacto del registro.
-- =============================================================================

-- 0) Comprobar que el usuario existe (si 0 filas, estás en otra BD o el login no coincide)
SELECT u.usu_id, u.usu_login, u.usu_estado, r.rol_descripcion
FROM auth.usuarios u
LEFT JOIN auth.usuario_roles ur ON ur.usu_id = u.usu_id AND ur.usu_rol_estado = 'A'
LEFT JOIN auth.roles r ON r.rol_id = ur.rol_id
WHERE u.usu_login = 'TU_CORREO@ejemplo.com';

-- 1) Añadir rol ADMIN (mantiene CLIENTE; el JWT llevará ambos tras volver a iniciar sesión)
INSERT INTO auth.usuario_roles (usu_id, rol_id, usu_rol_estado)
SELECT u.usu_id, r.rol_id, 'A'
FROM auth.usuarios u
CROSS JOIN auth.roles r
WHERE u.usu_login = 'TU_CORREO@ejemplo.com'
  AND r.rol_descripcion = 'ADMIN'
  AND r.rol_estado = 'A'
ON CONFLICT (usu_id, rol_id) DO UPDATE SET usu_rol_estado = 'A';

-- 2) OPCIONAL: quitar solo CLIENTE y dejar únicamente ADMIN
-- DELETE FROM auth.usuario_roles ur
-- USING auth.usuarios u, auth.roles r
-- WHERE ur.usu_id = u.usu_id
--   AND ur.rol_id = r.rol_id
--   AND u.usu_login = 'TU_CORREO@ejemplo.com'
--   AND r.rol_descripcion = 'CLIENTE';

-- 3) Verificar
SELECT u.usu_login, r.rol_descripcion
FROM auth.usuarios u
JOIN auth.usuario_roles ur ON ur.usu_id = u.usu_id AND ur.usu_rol_estado = 'A'
JOIN auth.roles r ON r.rol_id = ur.rol_id
WHERE u.usu_login = 'TU_CORREO@ejemplo.com';
