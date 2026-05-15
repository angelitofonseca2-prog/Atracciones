-- ETL idempotente (Fase 3): copia catálogos desde atracciones.* hacia catalogos.* (misma instancia PostgreSQL).
-- Requisito: esquema catalogos ya creado (migraciones EF de ms-catalogos). Usuario ETL con permisos INSERT.

BEGIN;

INSERT INTO catalogos.destinos (
    des_guid,
    des_nombre,
    des_pais,
    des_imagen_url,
    des_fecha_ingreso,
    des_usuario_ingreso,
    des_ip_ingreso,
    des_fecha_mod,
    des_usuario_mod,
    des_ip_mod,
    des_fecha_eliminacion,
    des_usuario_eliminacion,
    des_ip_eliminacion,
    des_estado
)
SELECT
    d.des_guid,
    d.des_nombre,
    d.des_pais,
    d.des_imagen_url,
    d.des_fecha_ingreso,
    d.des_usuario_ingreso,
    d.des_ip_ingreso,
    d.des_fecha_mod,
    d.des_usuario_mod,
    d.des_ip_mod,
    d.des_fecha_eliminacion,
    d.des_usuario_eliminacion,
    d.des_ip_eliminacion,
    d.des_estado
FROM atracciones.destino d
ON CONFLICT (des_guid) DO UPDATE SET
    des_nombre = EXCLUDED.des_nombre,
    des_pais = EXCLUDED.des_pais,
    des_imagen_url = EXCLUDED.des_imagen_url,
    des_fecha_ingreso = EXCLUDED.des_fecha_ingreso,
    des_usuario_ingreso = EXCLUDED.des_usuario_ingreso,
    des_ip_ingreso = EXCLUDED.des_ip_ingreso,
    des_fecha_mod = EXCLUDED.des_fecha_mod,
    des_usuario_mod = EXCLUDED.des_usuario_mod,
    des_ip_mod = EXCLUDED.des_ip_mod,
    des_fecha_eliminacion = EXCLUDED.des_fecha_eliminacion,
    des_usuario_eliminacion = EXCLUDED.des_usuario_eliminacion,
    des_ip_eliminacion = EXCLUDED.des_ip_eliminacion,
    des_estado = EXCLUDED.des_estado;

WITH RECURSIVE arbol AS (
    SELECT c.cat_id, c.cat_guid, c.cat_parent_id, c.cat_nombre, c.cat_fecha_ingreso,
           c.cat_usuario_ingreso, c.cat_ip_ingreso, c.cat_fecha_mod, c.cat_usuario_mod,
           c.cat_ip_mod, c.cat_fecha_eliminacion, c.cat_usuario_eliminacion, c.cat_ip_eliminacion,
           c.cat_estado, 0 AS nivel
    FROM atracciones.categoria c
    WHERE c.cat_parent_id IS NULL
    UNION ALL
    SELECT c.cat_id, c.cat_guid, c.cat_parent_id, c.cat_nombre, c.cat_fecha_ingreso,
           c.cat_usuario_ingreso, c.cat_ip_ingreso, c.cat_fecha_mod, c.cat_usuario_mod,
           c.cat_ip_mod, c.cat_fecha_eliminacion, c.cat_usuario_eliminacion, c.cat_ip_eliminacion,
           c.cat_estado, arbol.nivel + 1
    FROM atracciones.categoria c
    INNER JOIN arbol ON c.cat_parent_id = arbol.cat_id
)
INSERT INTO catalogos.categorias (
    cat_guid,
    cat_parent_guid,
    cat_nombre,
    cat_fecha_ingreso,
    cat_usuario_ingreso,
    cat_ip_ingreso,
    cat_fecha_mod,
    cat_usuario_mod,
    cat_ip_mod,
    cat_fecha_eliminacion,
    cat_usuario_eliminacion,
    cat_ip_eliminacion,
    cat_estado
)
SELECT DISTINCT ON (w.cat_guid)
    w.cat_guid,
    p.cat_guid AS cat_parent_guid,
    w.cat_nombre,
    w.cat_fecha_ingreso,
    w.cat_usuario_ingreso,
    w.cat_ip_ingreso,
    w.cat_fecha_mod,
    w.cat_usuario_mod,
    w.cat_ip_mod,
    w.cat_fecha_eliminacion,
    w.cat_usuario_eliminacion,
    w.cat_ip_eliminacion,
    w.cat_estado
FROM arbol w
LEFT JOIN atracciones.categoria p ON p.cat_id = w.cat_parent_id
ORDER BY w.cat_guid, w.nivel
ON CONFLICT (cat_guid) DO UPDATE SET
    cat_parent_guid = EXCLUDED.cat_parent_guid,
    cat_nombre = EXCLUDED.cat_nombre,
    cat_fecha_ingreso = EXCLUDED.cat_fecha_ingreso,
    cat_usuario_ingreso = EXCLUDED.cat_usuario_ingreso,
    cat_ip_ingreso = EXCLUDED.cat_ip_ingreso,
    cat_fecha_mod = EXCLUDED.cat_fecha_mod,
    cat_usuario_mod = EXCLUDED.cat_usuario_mod,
    cat_ip_mod = EXCLUDED.cat_ip_mod,
    cat_fecha_eliminacion = EXCLUDED.cat_fecha_eliminacion,
    cat_usuario_eliminacion = EXCLUDED.cat_usuario_eliminacion,
    cat_ip_eliminacion = EXCLUDED.cat_ip_eliminacion,
    cat_estado = EXCLUDED.cat_estado;

INSERT INTO catalogos.idiomas (
    id_guid,
    id_descripcion,
    id_fecha_ingreso,
    id_usuario_ingreso,
    id_ip_ingreso,
    id_fecha_mod,
    id_usuario_mod,
    id_ip_mod,
    id_fecha_eliminacion,
    id_usuario_eliminacion,
    id_ip_eliminacion,
    id_estado
)
SELECT
    i.id_guid,
    i.id_descripcion,
    i.id_fecha_ingreso,
    i.id_usuario_ingreso,
    i.id_ip_ingreso,
    i.id_fecha_mod,
    i.id_usuario_mod,
    i.id_ip_mod,
    i.id_fecha_eliminacion,
    i.id_usuario_eliminacion,
    i.id_ip_eliminacion,
    i.id_estado
FROM atracciones.idioma i
ON CONFLICT (id_guid) DO UPDATE SET
    id_descripcion = EXCLUDED.id_descripcion,
    id_fecha_ingreso = EXCLUDED.id_fecha_ingreso,
    id_usuario_ingreso = EXCLUDED.id_usuario_ingreso,
    id_ip_ingreso = EXCLUDED.id_ip_ingreso,
    id_fecha_mod = EXCLUDED.id_fecha_mod,
    id_usuario_mod = EXCLUDED.id_usuario_mod,
    id_ip_mod = EXCLUDED.id_ip_mod,
    id_fecha_eliminacion = EXCLUDED.id_fecha_eliminacion,
    id_usuario_eliminacion = EXCLUDED.id_usuario_eliminacion,
    id_ip_eliminacion = EXCLUDED.id_ip_eliminacion,
    id_estado = EXCLUDED.id_estado;

INSERT INTO catalogos.incluye (inc_guid, inc_descripcion, inc_estado)
SELECT inc.inc_guid, inc.inc_descripcion, inc.inc_estado
FROM atracciones.incluye inc
ON CONFLICT (inc_guid) DO UPDATE SET
    inc_descripcion = EXCLUDED.inc_descripcion,
    inc_estado = EXCLUDED.inc_estado;

INSERT INTO catalogos.imagenes (
    img_guid,
    img_url,
    img_descripcion,
    img_fecha_ingreso,
    img_usuario_ingreso,
    img_ip_ingreso,
    img_fecha_mod,
    img_usuario_mod,
    img_ip_mod,
    img_fecha_eliminacion,
    img_usuario_eliminacion,
    img_ip_eliminacion,
    img_estado
)
SELECT
    m.img_guid,
    m.img_url,
    m.img_descripcion,
    m.img_fecha_ingreso,
    m.img_usuario_ingreso,
    m.img_ip_ingreso,
    m.img_fecha_mod,
    m.img_usuario_mod,
    m.img_ip_mod,
    m.img_fecha_eliminacion,
    m.img_usuario_eliminacion,
    m.img_ip_eliminacion,
    m.img_estado
FROM atracciones.imagen m
ON CONFLICT (img_guid) DO UPDATE SET
    img_url = EXCLUDED.img_url,
    img_descripcion = EXCLUDED.img_descripcion,
    img_fecha_ingreso = EXCLUDED.img_fecha_ingreso,
    img_usuario_ingreso = EXCLUDED.img_usuario_ingreso,
    img_ip_ingreso = EXCLUDED.img_ip_ingreso,
    img_fecha_mod = EXCLUDED.img_fecha_mod,
    img_usuario_mod = EXCLUDED.img_usuario_mod,
    img_ip_mod = EXCLUDED.img_ip_mod,
    img_fecha_eliminacion = EXCLUDED.img_fecha_eliminacion,
    img_usuario_eliminacion = EXCLUDED.img_usuario_eliminacion,
    img_ip_eliminacion = EXCLUDED.img_ip_eliminacion,
    img_estado = EXCLUDED.img_estado;

COMMIT;
