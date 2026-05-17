-- Verificación rápida: todos los esquemas EF en un Postgres compartido (Railway).

SELECT current_database() AS database_name;

SELECT table_schema, COUNT(*) AS tablas
FROM information_schema.tables
WHERE table_schema IN ('auth', 'crm', 'ventas', 'inventario', 'catalogos', 'orq', 'billing', 'audit')
  AND table_type = 'BASE TABLE'
  AND table_name NOT LIKE '__EF%'
GROUP BY table_schema
ORDER BY table_schema;

SELECT table_schema, "MigrationId"
FROM (
    SELECT 'auth' AS table_schema, "MigrationId" FROM auth."__EFMigrationsHistory"
    UNION ALL SELECT 'crm', "MigrationId" FROM crm."__EFMigrationsHistory"
    UNION ALL SELECT 'ventas', "MigrationId" FROM ventas."__EFMigrationsHistory"
    UNION ALL SELECT 'inventario', "MigrationId" FROM inventario."__EFMigrationsHistory"
    UNION ALL SELECT 'catalogos', "MigrationId" FROM catalogos."__EFMigrationsHistory"
    UNION ALL SELECT 'orq', "MigrationId" FROM orq."__EFMigrationsHistory"
    UNION ALL SELECT 'billing', "MigrationId" FROM billing."__EFMigrationsHistory"
    UNION ALL SELECT 'audit', "MigrationId" FROM audit."__EFMigrationsHistory"
) h
ORDER BY table_schema, "MigrationId";
