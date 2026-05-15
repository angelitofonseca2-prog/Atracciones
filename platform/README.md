# Plataforma (Strangler Fig)

## Contenido

- **`gateway/`** — YARP: `POST /api/v1/auth/login` → **ms-identidad**; resto de `/api/**` → monolito.
- **`shared/BuildingBlocks`** — Correlación e idempotencia (stub).
- **`shared/Contracts.Protos`** — Incluye `usuario_service.proto` (gRPC identidad).
- **`docker-compose.yml`** — **postgres-identidad**, **ms-identidad**, **gateway**, **Jaeger**.

## Requisitos

- .NET 10 SDK
- Monolito en **`http://localhost:5031`** (si desarrollas API en el host).
- Docker (opcional).

Solución .NET plataforma: **`Atracciones.Platform.slnx`**. Solución identidad: [`../services/ms-identidad/Atracciones.MsIdentidad.slnx`](../services/ms-identidad/Atracciones.MsIdentidad.slnx).

## Desarrollo local típico (Fase 1)

1. Postgres + identidad (o solo Postgres):

   ```powershell
   cd platform
   docker compose up -d postgres-identidad
   ```

2. **ms-identidad** (puerto **5101**):

   ```powershell
   cd ..\services\ms-identidad\src\Atracciones.MsIdentidad.Api
   dotnet run --launch-profile http
   ```

3. **Monolito** (5031) con `JwtSettings:JwksUrl` y `Identidad` en `appsettings.Development.json`.

4. **Gateway** (5000 con `dotnet run`; en Docker Compose el host usa **5050**):

   ```powershell
   cd platform\gateway
   dotnet run --launch-profile http
   ```

5. Frontend: `VITE_API_URL=http://localhost:5000/api/v1` en `.env.local`.

## Docker Compose completo

```powershell
cd platform
docker compose up -d --build
```

Levanta Postgres (5433), ms-identidad (5101), gateway en el host (**http://localhost:5050**), Jaeger. El monolito puede seguir en el host (`host.docker.internal:5031`).

**Windows:** si `5000:8080` falla con “forbidden by its access permissions”, el compose ya mapea el gateway a **5050**. Usa en el frontend `VITE_API_URL=http://localhost:5050/api/v1`.

## CORS

Igual que antes: orígenes Vite en `gateway/appsettings.Development.json` o variables `Cors__*`.

## Railway (build fallido / monorepo)

1. **Raíz del servicio (Root Directory):** déjala **vacía** (raíz del repositorio). Si la pones en `platform/gateway` o `frontend-atracciones`, los `COPY platform/...` o `COPY frontend-atracciones/...` del Dockerfile **fallan** porque el contexto ya no incluye esas rutas.
2. **Dockerfile / Railpack:** en la raíz del repo hay **`railway.json`** que fuerza el builder **Dockerfile** y apunta al monolito (`MicroservicioAtracionesAPI/Dockerfile`). Así Railway no intenta Railpack al importar el repo. Si añades **otro** servicio (gateway, `ms-identidad`, etc.), en ese servicio configura **`RAILWAY_DOCKERFILE_PATH`** (p. ej. `platform/gateway/Dockerfile`) y, si Railway aplica el `railway.json` de la raíz a todos los servicios, usa **config as code por servicio** según [monorepo](https://docs.railway.com/deployments/monorepo) o anula en el panel el Dockerfile de ese servicio.
3. **Build logs:** si sigue fallando, abre la pestaña **Build Logs** del despliegue; suele verse `COPY failed` (contexto) o error de `dotnet publish`.
4. En la raíz del repo hay **`.dockerignore`** para que `COPY . .` no suba `node_modules`, `.git`, `bin/obj`, etc. (evita timeouts en Railway).

## Siguiente fase

[Fase 2 en AGENTS.md](../AGENTS.md) — `ms-clientes`.
