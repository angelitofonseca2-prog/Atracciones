# PostgreSQL en Railway (Atracciones)

## Qué significan los errores del log

| Mensaje en Data UI | Causa |
|--------------------|--------|
| `relation "auth_db" does not exist` | Se usó `auth_db` como **tabla**. `auth_db` es solo el nombre de la BD en Docker local, no un esquema. |
| `relation "auth_db.usuarios" does not exist` | Sintaxis inválida en Postgres. El esquema es `auth`, la tabla `usuarios`. |
| `relation "auth.usuarios" does not exist` | La BD conectada **no tiene migraciones** de `ms-identidad` (servicio no arrancó contra esa BD o falló al migrar). |

Consulta correcta:

```sql
SELECT * FROM auth.usuarios LIMIT 10;
```

## Topología recomendada (un solo Postgres)

En el plan típico de Railway hay **una** instancia Postgres y varios microservicios. Todos deben:

1. Tener **variable de referencia** `DATABASE_URL` (o `ConnectionStrings__*`) apuntando a **esa misma** instancia.
2. Arrancar al menos una vez para que EF cree esquemas:

| Servicio | Esquemas EF |
|----------|-------------|
| ms-identidad | `auth` |
| ms-reservas | `crm`, `ventas` |
| ms-atracciones | `catalogos`, `inventario` |
| ms-orquestador | `orq` |
| ms-facturacion | `billing` |
| ms-auditoria | `audit` |

No hace falta un Postgres por microservicio: conviven esquemas distintos en la misma base `railway`.

## Orden de despliegue tras vaciar la BD

1. **Postgres** (plugin)
2. **ms-identidad** — obligatorio antes del registro (`auth.usuarios`). Configurar `Jwt__RsaPrivateKeyPem` o `Jwt__RsaPrivateKeyPath` en producción.
3. **ms-reservas** — perfil CRM (`crm.clientes`)
4. **ms-atracciones**, **ms-orquestador**, resto

## Comprobar sin Data UI

- `GET https://<ms-identidad>/health/db` → `{ "status": "ok", "schema": "auth" }`
- `GET https://<ms-reservas>/health/db` → esquemas `crm` y `ventas`

Si devuelve 503, revisar logs de arranque del servicio (migración fallida o `DATABASE_URL` ausente).

## Script SQL de verificación

[`services/ms-identidad/db/verify_railway_auth.sql`](../../services/ms-identidad/db/verify_railway_auth.sql)
