# Plataforma (Strangler Fig)

## Contenido

- **`gateway/`** — YARP: rutas a microservicios (sin fallback al monolito).
- **`marketplace-gateway/`** — Hot Chocolate GraphQL (:5200): catálogo + reserva async vía RabbitMQ.
- **`shared/BuildingBlocks`** — Correlación, idempotencia, defaults gRPC, **EvBus** (RabbitMQ, outbox).
- **`shared/Contracts.Events`** — Payloads y routing keys del bus.
- **`shared/Contracts.Protos`** — Contratos gRPC compartidos.
- **`docker-compose.yml`** — Postgres por servicio, **RabbitMQ**, microservicios, gateway REST, marketplace-gateway, Jaeger.

## URL única de desarrollo

- **Gateway REST:** `http://localhost:5050/api/v2` (Docker Compose y `dotnet run` del gateway con perfil `http`).
- **GraphQL marketplace:** `http://localhost:5200/graphql` (solo frontend público con `VITE_USE_GRAPHQL=true`).
- **Frontend:** `VITE_API_URL=http://localhost:5050/api/v2`, `VITE_GRAPHQL_URL=http://localhost:5200/graphql` en [`frontend-atracciones/.env.local`](../frontend-atracciones/.env.local).
- **Booking externo:** misma base `/api/v2`; ver [`docs/api/Endpoints-Booking-Atracciones.md`](../docs/api/Endpoints-Booking-Atracciones.md).

El monolito (`MicroservicioAtracionesAPI`, puerto 5031) es **legacy** y no se enruta desde el gateway. Solo usarlo como referencia o para ETL puntual.

## Requisitos

- .NET 10 SDK
- Docker (opcional, recomendado)

Solución plataforma: **`Atracciones.Platform.slnx`**.

## Desarrollo local (microservicios)

1. Infra y servicios:

   ```powershell
   cd platform
   docker compose up -d --build
   ```

   Expone gateway en **5050**, GraphQL en **5200**, RabbitMQ management **15672**, Jaeger, Postgres y microservicios según `docker-compose.yml`.

2. O bien servicios sueltos con `dotnet run` en cada `services/ms-*/src/*.Api` y gateway en `platform/gateway` (perfil **5050**).

3. Frontend:

   ```powershell
   cd frontend-atracciones
   npm run dev
   ```

## Contrato Booking público

Ver [`MicroservicioAtracionesAPI/docs/api/openapi-v2-booking-public.md`](../MicroservicioAtracionesAPI/docs/api/openapi-v2-booking-public.md).

Flujo de pago: `POST /reservas` → `POST /pagos/paypal/orders` (opcional) → `POST /reservas/{guid}/pagos/confirmacion`.

## CORS

Orígenes Vite en `gateway/appsettings.Development.json` o variables `Cors__*`.

## Railway

Guía de BD: [`docs/railway-database.md`](docs/railway-database.md).

Variables por servicio: ver sección Railway al final del README histórico en commits anteriores o `AGENTS.md` (gRPC `*.railway.internal`, `GrpcClients__*` del orquestador).
