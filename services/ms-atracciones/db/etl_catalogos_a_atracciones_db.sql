-- ETL idempotente: migra los datos del esquema catalogos (BD catalogo original)
-- al esquema catalogos dentro de atracciones_db.
-- Ejecutar con acceso a AMBAS instancias usando dblink o psql --set variables.
--
-- Prerequisito: La migración EF de CatalogosDbContext ya creó el esquema catalogos
-- en atracciones_db con las tablas correspondientes.
--
-- Uso ejemplo:
--   psql "host=localhost port=5436 dbname=atracciones_db user=inventario password=inventario" \
--        -f etl_catalogos_a_atracciones_db.sql
--
-- El script asume que se ejecuta DENTRO de atracciones_db y que catalogos_source_*
-- son variables de conexión dblink (ajustar según entorno).
--
-- NOTA: Si la BD origen ya era la misma instancia postgres con otra base de datos,
-- usar INSERT ... ON CONFLICT DO UPDATE (UPSERT) para idempotencia.

-- ============================================================
-- PASO 1 — Crear extensión dblink si no existe
-- ============================================================
CREATE EXTENSION IF NOT EXISTS dblink;

-- ============================================================
-- Variables de conexión (ajustar contraseña/host si difiere)
-- ============================================================
-- Fuente: la BD original de ms-catalogos (si aún existe en otra instancia)
-- Si ya fusionaste los datos, puedes saltar este script.
DO $$
DECLARE
    src_conn TEXT := 'host=localhost port=5302 dbname=catalogos user=catalogos password=catalogos';
BEGIN

-- ============================================================
-- PASO 2 — Destinos
-- ============================================================
INSERT INTO catalogos.destinos (
    des_guid, des_nombre, des_pais, des_imagen_url,
    des_fecha_ingreso, des_usuario_ingreso, des_ip_ingreso, des_estado
)
SELECT
    des_guid::uuid, des_nombre, des_pais, des_imagen_url,
    des_fecha_ingreso, des_usuario_ingreso, des_ip_ingreso, des_estado::char
FROM dblink(src_conn,
    'SELECT des_guid, des_nombre, des_pais, des_imagen_url,
            des_fecha_ingreso, des_usuario_ingreso, des_ip_ingreso, des_estado
     FROM catalogos.destinos')
AS t(des_guid text, des_nombre text, des_pais text, des_imagen_url text,
     des_fecha_ingreso timestamptz, des_usuario_ingreso text, des_ip_ingreso text, des_estado char)
ON CONFLICT (des_guid) DO UPDATE SET
    des_nombre          = EXCLUDED.des_nombre,
    des_pais            = EXCLUDED.des_pais,
    des_imagen_url      = EXCLUDED.des_imagen_url,
    des_estado          = EXCLUDED.des_estado,
    des_fecha_mod       = NOW() AT TIME ZONE 'UTC';

-- ============================================================
-- PASO 3 — Categorías (sin FK FK padre garantizada: insertar en orden)
-- ============================================================
INSERT INTO catalogos.categorias (
    cat_guid, cat_nombre, cat_parent_guid,
    cat_fecha_ingreso, cat_usuario_ingreso, cat_ip_ingreso, cat_estado
)
SELECT
    cat_guid::uuid, cat_nombre,
    NULLIF(cat_parent_guid, '')::uuid,
    cat_fecha_ingreso, cat_usuario_ingreso, cat_ip_ingreso, cat_estado::char
FROM dblink(src_conn,
    'SELECT cat_guid, cat_nombre, COALESCE(cat_parent_guid::text, '''') as cat_parent_guid,
            cat_fecha_ingreso, cat_usuario_ingreso, cat_ip_ingreso, cat_estado
     FROM catalogos.categorias
     ORDER BY cat_parent_guid NULLS FIRST')
AS t(cat_guid text, cat_nombre text, cat_parent_guid text,
     cat_fecha_ingreso timestamptz, cat_usuario_ingreso text, cat_ip_ingreso text, cat_estado char)
ON CONFLICT (cat_guid) DO UPDATE SET
    cat_nombre      = EXCLUDED.cat_nombre,
    cat_parent_guid = EXCLUDED.cat_parent_guid,
    cat_estado      = EXCLUDED.cat_estado,
    cat_fecha_mod   = NOW() AT TIME ZONE 'UTC';

-- ============================================================
-- PASO 4 — Idiomas
-- ============================================================
INSERT INTO catalogos.idiomas (
    id_guid, id_descripcion,
    id_fecha_ingreso, id_usuario_ingreso, id_ip_ingreso, id_estado
)
SELECT
    id_guid::uuid, id_descripcion,
    id_fecha_ingreso, id_usuario_ingreso, id_ip_ingreso, id_estado::char
FROM dblink(src_conn,
    'SELECT id_guid, id_descripcion,
            id_fecha_ingreso, id_usuario_ingreso, id_ip_ingreso, id_estado
     FROM catalogos.idiomas')
AS t(id_guid text, id_descripcion text,
     id_fecha_ingreso timestamptz, id_usuario_ingreso text, id_ip_ingreso text, id_estado char)
ON CONFLICT (id_guid) DO UPDATE SET
    id_descripcion  = EXCLUDED.id_descripcion,
    id_estado       = EXCLUDED.id_estado,
    id_fecha_mod    = NOW() AT TIME ZONE 'UTC';

-- ============================================================
-- PASO 5 — Incluye
-- ============================================================
INSERT INTO catalogos.incluye (inc_guid, inc_descripcion, inc_estado)
SELECT
    inc_guid::uuid, inc_descripcion, inc_estado::char
FROM dblink(src_conn,
    'SELECT inc_guid, inc_descripcion, inc_estado FROM catalogos.incluye')
AS t(inc_guid text, inc_descripcion text, inc_estado char)
ON CONFLICT (inc_guid) DO UPDATE SET
    inc_descripcion = EXCLUDED.inc_descripcion,
    inc_estado      = EXCLUDED.inc_estado;

-- ============================================================
-- PASO 6 — Imágenes
-- ============================================================
INSERT INTO catalogos.imagenes (
    img_guid, img_url, img_descripcion,
    img_fecha_ingreso, img_usuario_ingreso, img_ip_ingreso, img_estado
)
SELECT
    img_guid::uuid, img_url, img_descripcion,
    img_fecha_ingreso, img_usuario_ingreso, img_ip_ingreso, img_estado::char
FROM dblink(src_conn,
    'SELECT img_guid, img_url, img_descripcion,
            img_fecha_ingreso, img_usuario_ingreso, img_ip_ingreso, img_estado
     FROM catalogos.imagenes')
AS t(img_guid text, img_url text, img_descripcion text,
     img_fecha_ingreso timestamptz, img_usuario_ingreso text, img_ip_ingreso text, img_estado char)
ON CONFLICT (img_guid) DO UPDATE SET
    img_url         = EXCLUDED.img_url,
    img_descripcion = EXCLUDED.img_descripcion,
    img_estado      = EXCLUDED.img_estado,
    img_fecha_mod   = NOW() AT TIME ZONE 'UTC';

RAISE NOTICE 'ETL catalogos completado exitosamente.';
END $$;
