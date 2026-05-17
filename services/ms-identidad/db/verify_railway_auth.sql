-- Verificación en Railway Data / psql (NO usar auth_db como tabla).
-- auth_db es el *nombre de la base* en Docker local; en Railway suele ser "railway" u otro.

-- 1) Base y esquemas presentes
SELECT current_database() AS database_name;
SELECT schema_name
FROM information_schema.schemata
WHERE schema_name IN ('auth', 'crm', 'ventas', 'inventario', 'catalogos', 'orq', 'billing', 'audit')
ORDER BY 1;

-- 2) Tablas de identidad (registro falla si esto está vacío o no existe)
SELECT table_schema, table_name
FROM information_schema.tables
WHERE table_schema = 'auth'
ORDER BY table_name;

-- 3) Datos (tras un registro exitoso debería haber filas)
SELECT COUNT(*) AS usuarios FROM auth.usuarios;
SELECT rol_descripcion FROM auth.roles WHERE rol_estado = 'A';

-- 4) Historial EF (debe existir tras arrancar ms-identidad)
SELECT * FROM auth."__EFMigrationsHistory" ORDER BY "MigrationId";
