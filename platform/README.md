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

## Siguiente fase

[Fase 2 en AGENTS.md](../AGENTS.md) — `ms-clientes`.
