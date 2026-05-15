# ms-catalogos

Microservicio **.NET 10** para catálogos (destinos, categorías, idiomas, incluye, imágenes): BD propia PostgreSQL esquema `catalogos`, REST admin alineado al monolito (`/api/v1/admin/...`), gRPC `CatalogoService.GetCatalogosPorGuids`, y mirror HTTP opcional al monolito (`POST /internal/v1/catalogos/mirror`) para mantener `atracciones.*` hasta Fase 4.

## Ejecución local

- Postgres catálogo: puerto host **5435**, BD `catalogos`, usuario/clave `catalogos` (véase `appsettings.Development.json`). Arranque rápido: `docker compose -f platform/docker-compose.yml up -d postgres-catalog`.
- API: **5301** (`launchSettings`). Use **`dotnet run --launch-profile http`** desde `src/Atracciones.MsCatalogos.Api` (o Visual Studio con perfil `http`); si ejecuta `dotnet run` sin perfil en entorno Production, la cadena `CatalogosDb` viene vacía y la API no arranca (mensaje explícito en consola).

### Si el navegador muestra "connection failed" en `http://localhost:5301`

1. Compruebe que el proceso está en ejecución y escuchando (consola: `Now listening on: http://localhost:5301`).
2. Levante Postgres en **5435** antes de usar endpoints que consultan BD; `/health` responde aunque la migración falle (revise logs `CatalogosDb`).
- JWT JWKS: mismo issuer/audiencia que el resto del sistema (`Jwt__JwksUrl` → ms-identidad).
- Mirror saliente: `MonolithCatalogLegacy` (`Enabled`, `BaseUrl`, `SyncApiKey`); debe coincidir con `CatalogMirrorIngress:ApiKey` del monolito.

## ETL

Script idempotente: [`db/etl_catalogos_desde_atracciones.sql`](db/etl_catalogos_desde_atracciones.sql) (misma instancia `atracciones` + `catalogos`). Las categorías se migran por recorrido desde raíces (`cat_parent_id IS NULL`); filas no alcanzables desde una raíz no se copian.

## Docker

Imagen: [`Dockerfile`](Dockerfile) (contexto raíz del repo). Compose: `platform/docker-compose.yml` (`postgres-catalog`, `ms-catalogos`).
