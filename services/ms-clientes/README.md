# ms-clientes

Microservicio de **CRM ligero**: perfil del cliente (`GET`/`PUT /api/v1/clientes/perfil`), espejo interno desde el monolito (`POST /internal/v1/clientes/mirror`), gRPC `ClienteService` y Postgres esquema **`crm`**.

## Local

1. **Postgres CRM** en `localhost:5434` (usuario/clave/db `crm`). Desde la raíz del repo:  
   `docker compose -f platform/docker-compose.yml up -d postgres-crm`  
   Sin este contenedor, `dotnet run` falla al aplicar migraciones (`Failed to connect to 127.0.0.1:5434`).

**Historial EF en el esquema correcto:** el historial `__EFMigrationsHistory` se guarda en **`crm`** (no en `public`). Si alguna vez vaciaste solo `DROP SCHEMA crm CASCADE` y la app dijo “already up to date” sin crear tablas, borra también `public."__EFMigrationsHistory"` o ejecuta:

`DROP SCHEMA IF EXISTS crm CASCADE; DROP TABLE IF EXISTS public."__EFMigrationsHistory";`

y vuelve a arrancar la API para reaplicar migraciones.
2. **ms-identidad** en `localhost:5101` para JWKS (`Jwt:JwksUrl`). Si aún no está arriba, la API **igual arranca**: JWKS se reintenta cada 10 s hasta que responda; mientras tanto `/health` funciona y los endpoints con JWT devolverán 401 hasta cargar claves.
3. Ejecutar la API (`launchSettings`: puerto **5201`):  
   `dotnet run --launch-profile http` desde `src/Atracciones.MsClientes.Api`.

Prueba rápida en navegador: `http://localhost:5201/health` (JSON `{ "status": "ok" }`). La raíz `/` puede responder 404 si no hay página estática.

## Docker

Desde `platform/docker-compose.yml`: servicios `postgres-crm` y `ms-clientes`.

## ETL inicial

Tras crear el esquema `crm`, opcionalmente ejecutar [`db/etl_crm_desde_atracciones.sql`](db/etl_crm_desde_atracciones.sql) en una instancia que tenga ambos esquemas.
