# Marketplace GraphQL + EvBus — Estado e implementación

Documento operativo del plan adaptado desde RedCar (bus de eventos + GraphQL) al proyecto **Atracciones**. Complementa [migracion-esb-evbus-marketplace.md](migracion-esb-evbus-marketplace.md) (fundamentos) y [platform/docs/evbus-graphql-verificacion.md](../platform/docs/evbus-graphql-verificacion.md) (pruebas).

**Decisión arquitectónica:** doble vía de reservas.

| Vía | Cliente | Reserva | Pago / factura |
|-----|---------|---------|----------------|
| **A — REST legacy** | Booking externo, admin | `POST /api/v2/reservas` → orquestador gRPC síncrono → **201** | `POST .../pagos/confirmacion` → saga gRPC |
| **B — GraphQL + EvBus** | Frontend React (`VITE_USE_GRAPHQL=true`) | Mutation `solicitarReserva` → RabbitMQ → ms-reservas | REST confirmación pago (sin cambio); polling `estadoReserva` |

---

## 1. Completado (Fases 0–5)

### Fase 0 — Contratos e infraestructura compartida

| Componente | Ubicación |
|------------|-----------|
| Contratos de eventos | `platform/shared/Contracts.Events/` |
| RabbitMQ publisher/consumer, outbox, idempotencia | `platform/shared/BuildingBlocks/EventBus/` |
| Flag `EvBus:Enabled` | `AddAtraccionesEventBus()` |
| Exchange topic | `atracciones.events`; DLX `atracciones.dlx`; vhost `atracciones` |

Eventos marketplace: `marketplace.reserva.solicitada|confirmada|rechazada`, `reservas.reserva.pagada`, `atracciones.horario.cupo_agotado`.

### Fase 1 — Microservicios

| Servicio | Cambios |
|----------|---------|
| **ms-reservas** | Outbox, seguimiento marketplace, consumidor `marketplace.reserva.solicitada`, productor confirmada/rechazada, `GET /internal/v1/marketplace/reservas/{id}/estado` |
| **ms-atracciones** | Outbox, consumidor sync marketplace, productor `cupo_agotado` |
| **ms-auditoria** | Consumidor `marketplace.#` → `audit.eventos` (gRPC `RegistrarEvento` intacto) |
| **ms-facturacion** | Consumidor `reservas.reserva.pagada` (gRPC orquestador intacto; `EmitirAsync` idempotente por `rev_guid`) |
| **ms-orquestador** | Sin cambios de contrato REST Booking |

Migraciones EF manuales: `20260607120000` (reservas), `20260607120100` (atracciones), `20260607120200` (auditoría), `20260607120300` (facturación).

### Fase 2 — Docker Compose

- Servicio **rabbitmq** (5672, management 15672, user/pass `atracciones`)
- Servicio **marketplace-gateway** (:5200)
- Variables `EvBus__Enabled`, `RabbitMQ__*` en MS afectados y frontend (`VITE_GRAPHQL_URL`, `VITE_USE_GRAPHQL`)

### Fase 3 — Marketplace Gateway GraphQL

- `platform/marketplace-gateway/` — Hot Chocolate 15
- Queries (proxy HTTP ms-atracciones): `atracciones`, `filtros`, `atraccion`, `horarios`, `tickets`
- Query `estadoReserva` (proxy ms-reservas internal)
- Mutation `solicitarReserva` → publica `marketplace.reserva.solicitada`

### Fase 4 — Frontend React

- Apollo Client: `src/graphql/client.js`, `marketplaceApi.js`
- `ApolloProvider` en `main.jsx`
- Catálogo (`useAtracciones`, `useHomeDestacadas`) vía GraphQL si `VITE_USE_GRAPHQL=true`
- `ReservaPage`: reserva async + polling; horarios/tickets vía GraphQL cuando flag activo
- Fallback REST completo si `VITE_USE_GRAPHQL=false`

### Fase 5 — Verificación

- Builds: BuildingBlocks, marketplace-gateway, MS modificados, `npm run build`
- Guía: [platform/docs/evbus-graphql-verificacion.md](../platform/docs/evbus-graphql-verificacion.md)
- Solución: `platform/Atracciones.Platform.slnx` incluye Contracts.Events y marketplace-gateway

---

## 2. En progreso / recién cerrado

| Ítem | Estado |
|------|--------|
| Publicar `reservas.reserva.pagada` al confirmar pago (outbox ms-reservas) | Implementado en esta iteración |
| ReservaPage: horarios/tickets GraphQL | Implementado en esta iteración |
| UI “Procesando reserva…” durante polling async | Implementado en esta iteración |
| Documentación AGENTS Fase 8 + migracion-esb actualizada | Esta iteración |

---

## 3. Pendiente (roadmap Fase 6+)

Prioridad sugerida para siguientes sprints:

| # | Entregable | Notas |
|---|------------|-------|
| 1 | **E2E runtime** | `docker compose up`, flujo reserva GraphQL → RabbitMQ → CONFIRMADA → pago REST |
| 2 | **Railway producción** | Desplegar `marketplace-gateway`, RabbitMQ managed o CloudAMQP; `VITE_GRAPHQL_URL` en build frontend |
| 3 | **YARP opcional** | Ruta `/graphql` en gateway :5050 (hoy frontend llama directo :5200) |
| 4 | **Shadow mode facturación** | Validar consumidor `reservas.reserva.pagada` vs gRPC orquestador; luego retirar gRPC factura de saga |
| 5 | **ms-identidad** | Publicar `identidad.usuario.registrado`; ms-reservas consume |
| 6 | **Notificaciones Booking** | Webhook/cola B2B para eventos marketplace |
| 7 | **OpenTelemetry EvBus** | Spans publish/consume en BuildingBlocks |
| 8 | **Hot Chocolate** | Actualizar paquete (NU1904 en Language) |
| 9 | **EF snapshots** | Regenerar snapshots oficiales con `dotnet ef` para migraciones manuales |
| 10 | **Apagar monolito** | Strangler Fig final ([plan-fusion-microservicios.md](plan-fusion-microservicios.md)) |
| 11 | **B2B mTLS** | Client credentials Booking en gateway |

---

## 4. Rutas clave

```
platform/shared/Contracts.Events/
platform/shared/BuildingBlocks/EventBus/
platform/marketplace-gateway/
platform/docker-compose.yml
platform/docs/evbus-graphql-verificacion.md
services/ms-reservas/.../EventBus/
services/ms-atracciones/.../EventBus/
services/ms-auditoria/.../EventBus/
services/ms-facturacion/.../EventBus/
frontend-atracciones/src/graphql/
frontend-atracciones/src/config/graphqlUrl.js
```

---

## 5. Variables de entorno

### Backend (EvBus)

```json
"EvBus": { "Enabled": true },
"RabbitMQ": {
  "Host": "rabbitmq",
  "Port": 5672,
  "VirtualHost": "atracciones",
  "UserName": "atracciones",
  "Password": "atracciones"
}
```

### Frontend

```env
VITE_API_URL=http://localhost:5050/api/v2
VITE_GRAPHQL_URL=http://localhost:5200/graphql
VITE_USE_GRAPHQL=true
```

---

## 6. No-regresión (obligatorio en cada release)

- `POST /api/v2/reservas` → **201** (Booking)
- Admin `/admin/*` REST intacto
- `VITE_USE_GRAPHQL=false` → frontend 100 % REST

---

*Última actualización: junio 2026 — implementación híbrida saga gRPC + EvBus marketplace.*
