# ms-identidad (Fase 1)

Microservicio de autenticación: esquema PostgreSQL `auth`, JWT **RS256**, `/.well-known/jwks.json`, REST `POST /api/v2/auth/login`, gRPC `UsuarioService`, endpoint interno de espejo para el monolito.

## Requisitos

- .NET 10 SDK
- PostgreSQL 16+ (local `localhost:5433` si usas `docker compose` de `platform/`)

## Desarrollo local

1. Base de datos (ejemplo):

   ```powershell
   docker compose -f platform/docker-compose.yml up -d postgres-identidad
   ```

2. API:

   ```powershell
   cd services/ms-identidad/src/Atracciones.MsIdentidad.Api
   dotnet run --launch-profile http
   ```

3. Migraciones ya se aplican al arrancar (`Database.MigrateAsync`).

## Configuración clave

| Clave | Descripción |
|--------|-------------|
| `ConnectionStrings:IdentidadDb` | Npgsql hacia la BD `identidad` |
| `Jwt:Issuer` / `Jwt:Audience` | Deben coincidir con `JwtSettings` del monolito |
| `Jwt:RsaPrivateKeyPem` o `Jwt:RsaPrivateKeyPath` | Clave RSA de firma (obligatorio en producción) |
| `InternalSync:MonolithApiKey` | Cabecera `X-Monolith-Sync-Key` esperada en `POST /internal/v1/auth/mirror` |

En Development sin PEM, se genera una RSA efímera (los tokens dejan de ser válidos al reiniciar).

## ETL desde el monolito

Con ambos esquemas en la misma instancia, tras crear tablas `auth.*`:

`db/etl_auth_desde_atracciones.sql`

## Contrato gRPC

`platform/shared/Contracts.Protos/usuario_service.proto`
