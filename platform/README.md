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

## Railway — Configuración por servicio

### Variables obligatorias en Railway

Cada servicio lee `DATABASE_URL` y la convierte automáticamente al formato Npgsql.
Configura estas variables en la pestaña **Variables** de cada servicio:

| Servicio Railway | DATABASE_URL (termina en) | Variables adicionales obligatorias |
|---|---|---|
| `Atracciones` (monolito) | `/atracciones_db` | — |
| `services/ms-identidad` | `/auth_db` | `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpirationHours`, `Jwt__KeyId`, `Jwt__RsaPrivateKeyPem` (contenido del PEM), `InternalSync__MonolithApiKey` |
| `services/ms-atracciones` | `/atracciones_db` | `Jwt__Issuer`, `Jwt__Audience`, `Jwt__JwksUrl` |
| `services/ms-reservas` | `/reservas_db` | `Jwt__Issuer`, `Jwt__Audience`, `Jwt__JwksUrl`, `ClientesMirror__MonolithApiKey` |
| `services/ms-facturacion` | `/facturacion_db` | `Jwt__Issuer`, `Jwt__Audience`, `Jwt__JwksUrl` |
| `services/ms-orquestador` | `/orquestador_db` | `Jwt__Issuer`, `Jwt__Audience`, `Jwt__JwksUrl`, `GrpcClients__Identidad`, `GrpcClients__Atracciones`, `GrpcClients__Reservas`, `GrpcClients__Facturacion`, `GrpcClients__Auditoria`, `PayPal__ClientId`, `PayPal__ClientSecret`, `PayPal__WebhookId` |
| `services/ms-auditoria` | `/audit_db` | — |

**Valores de ejemplo para los JWT (usar los mismos en todos los servicios):**
```
Jwt__Issuer=microservicio-atracciones
Jwt__Audience=booking-prototipo
Jwt__JwksUrl=https://<dominio-publico-ms-identidad>/.well-known/jwks.json
```

**Nota sobre `Jwt__RsaPrivateKeyPem` en ms-identidad:**
El contenido del archivo `platform/secrets/dev-rsa-private.pem` (generado localmente) debe pegarse como valor de esta variable en Railway (incluyendo las líneas `-----BEGIN RSA PRIVATE KEY-----` y `-----END RSA PRIVATE KEY-----`). Sin esto, cada redeploy genera una clave distinta y los tokens existentes se invalidan.

**Nota sobre `GrpcClients__*` en ms-orquestador:**
Railway expone un único puerto HTTP por servicio. Usar el dominio público de cada microservicio con puerto 443 (Railway maneja TLS automáticamente). Ejemplo:
```
GrpcClients__Identidad=https://<dominio-ms-identidad>
GrpcClients__Atracciones=https://<dominio-ms-atracciones>
GrpcClients__Reservas=https://<dominio-ms-reservas>
GrpcClients__Facturacion=https://<dominio-ms-facturacion>
GrpcClients__Auditoria=https://<dominio-ms-auditoria>
```

### Root Directory y Dockerfile path

- **Root Directory**: dejar **vacío** en todos los servicios. Los Dockerfiles hacen `COPY . .` desde la raíz del repo; si se cambia el Root Directory, el contexto del build cambia y las rutas de `dotnet publish` dejan de existir.
- **Dockerfile path** (`RAILWAY_DOCKERFILE_PATH`): configurar por servicio:

| Servicio | RAILWAY_DOCKERFILE_PATH |
|---|---|
| `Atracciones` (monolito) | `MicroservicioAtracionesAPI/Dockerfile` (ya fijado en `railway.json`) |
| `services/ms-identidad` | `services/ms-identidad/Dockerfile` |
| `services/ms-atracciones` | `services/ms-atracciones/Dockerfile` |
| `services/ms-reservas` | `services/ms-reservas/Dockerfile` |
| `services/ms-facturacion` | `services/ms-facturacion/Dockerfile` |
| `services/ms-orquestador` | `services/ms-orquestador/Dockerfile` |
| `services/ms-auditoria` | `services/ms-auditoria/Dockerfile` |

---

## Railway (build fallido / monorepo)

1. **Raíz del servicio (Root Directory):** déjala **vacía** (raíz del repositorio). Si la pones en `platform/gateway` o `frontend-atracciones`, los `COPY platform/...` o `COPY frontend-atracciones/...` del Dockerfile **fallan** porque el contexto ya no incluye esas rutas.
2. **Dockerfile / Railpack:** en la raíz del repo hay **`railway.json`** que fuerza el builder **Dockerfile** y apunta al monolito (`MicroservicioAtracionesAPI/Dockerfile`). Así Railway no intenta Railpack al importar el repo. Si añades **otro** servicio (gateway, `ms-identidad`, etc.), en ese servicio configura **`RAILWAY_DOCKERFILE_PATH`** (p. ej. `platform/gateway/Dockerfile`) y, si Railway aplica el `railway.json` de la raíz a todos los servicios, usa **config as code por servicio** según [monorepo](https://docs.railway.com/deployments/monorepo) o anula en el panel el Dockerfile de ese servicio.
3. **Build logs:** si sigue fallando, abre la pestaña **Build Logs** del despliegue; suele verse `COPY failed` (contexto) o error de `dotnet publish`.
4. En la raíz del repo hay **`.dockerignore`** para que `COPY . .` no suba `node_modules`, `.git`, `bin/obj`, etc. (evita timeouts en Railway).

## Siguiente fase

[Fase 2 en AGENTS.md](../AGENTS.md) — `ms-clientes`.
