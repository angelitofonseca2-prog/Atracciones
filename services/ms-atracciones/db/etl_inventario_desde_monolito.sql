-- ETL plantilla (Fase 4): copiar inventario desde el monolito (PostgreSQL esquema atracciones.*)
-- hacia la BD dedicada de ms-atracciones (esquema inventario.*).
--
-- Requisitos:
--   1) Migraciones EF de ms-atracciones aplicadas (tablas inventario.*).
--   2) Ajustar nombres/columnas según el modelo real del monolito en cada entorno.
--   3) Los GUIDs de destino/categoría/idioma/imagen/incluye deben coincidir con ms-catalogos (Fase 3).
--
-- Ejecución típica: pg_dump del monolito + restore en staging, o FDW/dblink entre instancias.

BEGIN;

-- Ejemplo (comentado): core atracción + tablas puente
-- INSERT INTO inventario.atracciones (at_guid, des_guid, des_nombre_snap, des_pais_snap, ...)
-- SELECT at_guid, des_guid, des_nombre, des_pais, ...
-- FROM atracciones.atraccion WHERE at_estado = 'A'
-- ON CONFLICT (at_guid) DO UPDATE SET ...;

COMMIT;
