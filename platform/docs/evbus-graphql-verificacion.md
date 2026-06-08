# Verificación EvBus + GraphQL (Fase 5)

Ejecutar con Docker Compose levantado (`platform/docker-compose.yml`).

## Compilación (automática en CI/local)

```powershell
dotnet build platform/shared/BuildingBlocks/Atracciones.Platform.BuildingBlocks.csproj
dotnet build platform/marketplace-gateway/Atracciones.MarketplaceGateway.csproj
dotnet build services/ms-reservas/src/Atracciones.MsReservas.Api/Atracciones.MsReservas.Api.csproj
dotnet build services/ms-atracciones/src/Atracciones.MsAtracciones.Api/Atracciones.MsAtracciones.Api.csproj
dotnet build services/ms-auditoria/src/Atracciones.MsAuditoria.Api/Atracciones.MsAuditoria.Api.csproj
dotnet build services/ms-facturacion/src/Atracciones.MsFacturacion.Api/Atracciones.MsFacturacion.Api.csproj
cd frontend-atracciones; npm run build
```

## No-regresión REST Booking (vía A intacta)

- `POST http://localhost:5050/api/v2/reservas` → **201** con reserva `PENDIENTE` (sin cambios).
- `GET http://localhost:5050/api/v2/atracciones` → catálogo OK.
- Admin `/admin/*` → REST intacto.

## Marketplace GraphQL (vía B)

- Playground: `http://localhost:5200/graphql`
- Query `atracciones(page:1, limit:6)` → JSON envelope REST.
- Mutation `solicitarReserva` → estado `EN_PROCESO` + `seguimientoId`.
- Query `estadoReserva(seguimientoId)` → `CONFIRMADA` tras consumo RabbitMQ.

## RabbitMQ

- Management UI: `http://localhost:15672` (user `atracciones` / pass `atracciones`)
- Exchange `atracciones.events` y colas: `reservas.marketplace`, `audit.marketplace`, etc.
- Tras confirmar pago REST, ms-reservas encola `reservas.reserva.pagada` (shadow; facturación idempotente por `rev_guid`).

## Frontend

- `VITE_USE_GRAPHQL=true` → catálogo vía GraphQL; reserva async con polling.
- `VITE_USE_GRAPHQL=false` → fallback REST completo.
