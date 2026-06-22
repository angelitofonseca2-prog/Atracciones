# Variables de entorno Railway — Guía completa

Este archivo documenta **todas** las variables de entorno necesarias en cada
servicio Railway para que RabbitMQ, GraphQL (subscriptions incluidas),
gRPC y el resto del sistema funcionen correctamente en producción.

---

## Cómo crear el servicio RabbitMQ en Railway

1. En el proyecto Railway → **+ New** → **Database** → **RabbitMQ**.
2. El plugin crea variables automáticas:
   - `RABBITMQ_URL` (amqp://user:password@host:5672/vhost)
   - `RABBITMQ_DEFAULT_USER`, `RABBITMQ_DEFAULT_PASS`, `RABBITMQ_DEFAULT_VHOST`
3. Apunta el host interno: `rabbitmq.railway.internal` (puerto `5672`).
4. Panel de gestión (Management UI): actívalo en Railway con
   `RABBITMQ_PLUGINS=rabbitmq_management` y expón el puerto `15672`.
5. Crea el **virtual host** `atracciones` desde la Management UI o con:
   ```
   rabbitmqctl add_vhost atracciones
   rabbitmqctl set_permissions -p atracciones <user> ".*" ".*" ".*"
   ```

---

## Variables comunes de EvBus (copiar en cada servicio que lo necesite)

```
EvBus__Enabled=true
EvBus__Host=rabbitmq.railway.internal
EvBus__Port=5672
EvBus__VirtualHost=atracciones
EvBus__Username=<RABBITMQ_DEFAULT_USER de Railway>
EvBus__Password=<RABBITMQ_DEFAULT_PASS de Railway>
```

> **IMPORTANTE**: Mientras RabbitMQ no esté desplegado y probado, mantén
> `EvBus__Enabled=false` para que los servicios arranquen sin error.

---

## api-gateway

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# Destinos YARP — private networking Railway
ReverseProxy__Clusters__identidad__Destinations__d1__Address=http://ms-identidad.railway.internal:8080
ReverseProxy__Clusters__atracciones__Destinations__d1__Address=http://ms-atracciones.railway.internal:8080
ReverseProxy__Clusters__reservas__Destinations__d1__Address=http://ms-reservas.railway.internal:8080
ReverseProxy__Clusters__orquestador__Destinations__d1__Address=http://ms-orquestador.railway.internal:8080
ReverseProxy__Clusters__facturacion__Destinations__d1__Address=http://ms-facturacion.railway.internal:8080
ReverseProxy__Clusters__marketplace__Destinations__d1__Address=http://marketplace-gateway.railway.internal:5200

# CORS — orígenes permitidos (sin barra al final)
Cors__0=https://frontend-atracciones-production.up.railway.app
# Si tienes dominio personalizado:
# Cors__1=https://tudominio.com

Otlp__Endpoint=https://api.honeycomb.io   # o tu colector OTLP
```

---

## ms-identidad

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__AuthDb=Host=postgres.railway.internal;Database=auth_db;Username=...;Password=...
Jwt__Issuer=https://ms-identidad-production.up.railway.app
Jwt__Audience=atracciones-api
# Clave RS256 — genera con: openssl genrsa -out private.pem 2048
Jwt__PrivateKeyPem=<contenido del private.pem en una línea con \n>
# EvBus opcional (para auditoría de logins)
EvBus__Enabled=false
```

---

## ms-reservas

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__VentasDb=Host=postgres.railway.internal;Database=reservas_db;Username=...;Password=...
ConnectionStrings__CrmDb=Host=postgres.railway.internal;Database=reservas_db;Username=...;Password=...

# gRPC hacia ms-atracciones (puerto gRPC interno)
GrpcClients__Atracciones=http://ms-atracciones.railway.internal:8081

# JWT validación
Jwt__Authority=https://ms-identidad-production.up.railway.app
Jwt__Audience=atracciones-api

# EvBus — ACTIVAR cuando RabbitMQ esté listo
EvBus__Enabled=true
EvBus__Host=rabbitmq.railway.internal
EvBus__Port=5672
EvBus__VirtualHost=atracciones
EvBus__Username=<user>
EvBus__Password=<password>

Otlp__Endpoint=https://api.honeycomb.io
```

---

## ms-atracciones

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__InventarioDB=Host=postgres.railway.internal;Database=atracciones_db;Username=...;Password=...
ConnectionStrings__CatalogosDb=Host=postgres.railway.internal;Database=atracciones_db;Username=...;Password=...

Jwt__Authority=https://ms-identidad-production.up.railway.app
Jwt__Audience=atracciones-api

# EvBus — ACTIVAR cuando RabbitMQ esté listo
EvBus__Enabled=true
EvBus__Host=rabbitmq.railway.internal
EvBus__Port=5672
EvBus__VirtualHost=atracciones
EvBus__Username=<user>
EvBus__Password=<password>
```

---

## ms-orquestador

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__OrquestadorDb=Host=postgres.railway.internal;Database=orquestador_db;Username=...;Password=...

# gRPC clientes internos
GrpcClients__Reservas=http://ms-reservas.railway.internal:8081
GrpcClients__Clientes=http://ms-reservas.railway.internal:8081
GrpcClients__Atracciones=http://ms-atracciones.railway.internal:8081
GrpcClients__Facturacion=http://ms-facturacion.railway.internal:8081
GrpcClients__Identidad=http://ms-identidad.railway.internal:8081
GrpcClients__IdentidadHttp=http://ms-identidad.railway.internal:8080

Jwt__Authority=https://ms-identidad-production.up.railway.app
Jwt__Audience=atracciones-api

# EvBus — el orquestador no publica eventos (usa gRPC), mantener false
EvBus__Enabled=false

Otlp__Endpoint=https://api.honeycomb.io
```

---

## ms-facturacion

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__FacturacionDb=Host=postgres.railway.internal;Database=facturacion_db;Username=...;Password=...

Jwt__Authority=https://ms-identidad-production.up.railway.app
Jwt__Audience=atracciones-api

EvBus__Enabled=false
```

---

## ms-auditoria

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__AuditoriaDb=Host=postgres.railway.internal;Database=audit_db;Username=...;Password=...

# EvBus — puede activarse para recibir eventos de auditoría por bus
EvBus__Enabled=false

Otlp__Endpoint=https://api.honeycomb.io
```

---

## marketplace-gateway (GraphQL + Subscriptions WS)

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5200

# URLs internas a los microservicios
Services__AtraccionesHttp=http://ms-atracciones.railway.internal:8080
Services__ReservasHttp=http://ms-reservas.railway.internal:8080

# CORS — debe incluir el frontend y permitir credenciales (para WS)
Cors__0=https://frontend-atracciones-production.up.railway.app

# EvBus — ACTIVAR para que las subscriptions WS funcionen
EvBus__Enabled=true
EvBus__Host=rabbitmq.railway.internal
EvBus__Port=5672
EvBus__VirtualHost=atracciones
EvBus__Username=<user>
EvBus__Password=<password>

Otlp__Endpoint=https://api.honeycomb.io
```

### Variables de entorno Railway para marketplace-gateway → `railway.json`

```json
{
  "build": { "builder": "DOCKERFILE", "dockerfilePath": "platform/marketplace-gateway/Dockerfile" },
  "deploy": {
    "startCommand": "dotnet Atracciones.MarketplaceGateway.dll",
    "healthcheckPath": "/health"
  }
}
```

---

## frontend-atracciones

Variables en Railway **Settings → Variables** (se pasan como build args en Vite):

```
VITE_API_URL=https://api-gateway-production-0afd.up.railway.app/api/v2
VITE_USE_GRAPHQL=true
VITE_GRAPHQL_URL=https://marketplace-gateway-production.up.railway.app/graphql
```

> El cliente Apollo convierte automáticamente `https://…/graphql` en
> `wss://…/graphql` para las subscriptions WebSocket.

---

## Checklist de activación paso a paso

1. **Deploy RabbitMQ** en Railway → apunta las credenciales.
2. Crea el vhost `atracciones` y el exchange `atracciones.events` (el
   `RabbitMqTopologyInitializer` lo hace automáticamente al arrancar con `EvBus__Enabled=true`).
3. Activa `EvBus__Enabled=true` en: **ms-reservas**, **ms-atracciones**,
   **marketplace-gateway** (los principales productores/consumidores).
4. Deploy y revisa logs: busca `"Consumidor activo en cola"` y
   `"Exchange declarado"`.
5. Activa `VITE_USE_GRAPHQL=true` y `VITE_GRAPHQL_URL` en el frontend.
6. Prueba: crea una reserva y comprueba que el estado llega por WebSocket
   en la consola del navegador (`Network → WS → onEstadoReservaActualizado`).
7. Activa el **DLQ monitor**: si ves logs `[DLQ] Mensaje en dead-letter queue`
   investiga el mensaje completo; son indicadores de bugs en consumidores.

---

## Notas de troubleshooting

| Síntoma | Causa probable | Solución |
|---------|---------------|----------|
| `ECONNREFUSED` al publicar | EvBus__Enabled=true pero broker no disponible | Verificar host/port/vhost de RabbitMQ |
| Subscription WS no conecta | CORS no permite `AllowCredentials` + `AllowAnyOrigin` simultáneo | Usar `WithOrigins(...)` específico + `AllowCredentials()` |
| DLQ acumula mensajes | Consumer lanza excepción siempre → NACK → DLQ | Revisar log `[DLQ]` para el `event_type` que falla |
| `HTTP_1_1_REQUIRED` en gRPC | Proxy o Railway no envía HTTP/2 al servicio | Usar red privada `*.railway.internal` en lugar de URL pública |
| GraphQL query sin datos | `Services__AtraccionesHttp` apunta a URL incorrecta | Verificar URL interna con `/health` del microservicio |
| Subscription llega vacía | `ReservaEstadoEventConsumer` no activo (EvBus__Enabled=false) | Activar EvBus en marketplace-gateway |
