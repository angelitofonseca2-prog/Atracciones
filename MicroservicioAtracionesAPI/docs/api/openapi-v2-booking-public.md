# Contrato de Integración API de Atracciones v2.0.0 — Booking Público

> **Base URL:** `/api/v2`  
> **PDF normativo:** [`docs/api/Contrato_API_Atracciones_V2.pdf`](../../../docs/api/Contrato_API_Atracciones_V2.pdf)  
> **Guía operativa:** [`docs/api/Endpoints-Booking-Atracciones.md`](../../../docs/api/Endpoints-Booking-Atracciones.md)

---

## 1) Información general

| Campo | Valor |
|-------|--------|
| Nombre | API de Atracciones – Booking Público |
| Versión | 2.0.0 |
| Endpoint base | `/api/v2` |
| Formato | JSON |
| Moneda | USD |
| Paginación | `page` ≥ 1, `limit` 1–50 |

**Convenciones JSON**

- Dominio: **snake_case** (`rev_guid`, `nombre_receptor`).
- Metadatos de listado/filtros: **camelCase** (`filterStats`, `destinationFilters`).

**Servidores**

| Entorno | URL |
|---------|-----|
| Producción | `https://api-gateway-production-5c80b.up.railway.app/api/v2` |
| Local Docker | `http://localhost:5050/api/v2` |
| Local gateway | `http://localhost:5000/api/v2` |

---

## 2) Endpoints — Catálogo (1–6)

### Endpoint 1 — Listar atracciones

`GET /api/v2/atracciones`

| Query | Tipo | Req. | Descripción |
|-------|------|------|-------------|
| ciudad | string | No | Filtra por ciudad |
| tipo | string | No | cat_guid raíz |
| subtipo | string | No | cat_guid hijo |
| etiqueta | enum | No | `free_cancellation`, `skip_the_line` |
| idioma | enum | No | `en`, `es`, `fr`, … |
| calificacion_min | number | No | 3.0, 3.5, 4.0, 4.5 |
| horario | enum | No | `05:00-12:00`, `12:00-18:00`, `18:00-05:00` |
| disponible | boolean | No | Solo con disponibilidad |
| ordenar_por | enum | No | `trending` (default), `lowest_price`, `highest_weighted_rating` |
| page | integer | No | Default 1 |
| limit | integer | No | Default 10, max 50 |

**200 OK** — incluye `filterStats`, `sorters`, `defaultSorter`, `_links`.

**Errores:** `400`, `500`.

---

### Endpoint 2 — Filtros

`GET /api/v2/atracciones/filtros`

| Query | Tipo | Req. | Descripción |
|-------|------|------|-------------|
| ciudad | string | No | Filtra contadores por ciudad |

**200 OK** — `data.destinationFilters`, `typeFilters`, `labelFilters`, `minRatingFilter`, `timeOfDayFilters`, `supportedLanguageFilters`.

---

### Endpoint 3 — Detalle atracción

`GET /api/v2/atracciones/{guid}`

| Path | Tipo | Req. |
|------|------|------|
| guid | uuid | Sí |

**200 OK** — hereda campos de listado + `descripcion`, `imagenes`, `incluye`, `no_incluye`, `tickets[]`, `horarios_proximos[]`.

**Errores:** `404`, `500`.

---

### Endpoint 4 — Tickets de la atracción

`GET /api/v2/atracciones/{guid}/tickets`

**200 OK:**

```json
{
  "status": 200,
  "data": [
    { "tck_guid": "uuid", "tipo": "Adulto", "precio": 25.0, "moneda": "USD" }
  ]
}
```

---

### Endpoint 5 — Horarios

`GET /api/v2/atracciones/{guid}/horarios`

Por defecto solo horarios con cupo. Query opcional: `disponibles=false`.

**200 OK:**

```json
{
  "status": 200,
  "data": [
    {
      "hor_guid": "uuid",
      "fecha": "2026-06-01",
      "hora_inicio": "09:00",
      "hora_fin": "11:00",
      "cupos": 8
    }
  ]
}
```

---

### Endpoint 6 — Tickets por horario

`GET /api/v2/atracciones/{guid}/horarios/{horarioGuid}/tickets`

| Path | Tipo | Req. |
|------|------|------|
| guid | uuid | Sí |
| horarioGuid | uuid | Sí |

**200 OK:**

```json
{
  "status": 200,
  "message": "Consulta exitosa",
  "data": {
    "items": [
      { "tck_guid": "uuid", "tipo": "Adulto", "precio": 25.0, "moneda": "USD" }
    ]
  }
}
```

**Errores:** `404`, `500`.

---

## 3) Endpoints — Reservas (7–10)

### Endpoint 7 — Crear reserva

`POST /api/v2/reservas`

**Auth:** opcional. Sin JWT → `cliente_invitado` obligatorio. Con JWT → se ignora `cliente_invitado`.

**Headers recomendados:** `Content-Type: application/json`, `Idempotency-Key` (opcional).

**Body:**

```json
{
  "at_guid": "uuid",
  "hor_guid": "uuid",
  "lineas": [{ "tck_guid": "uuid", "cantidad": 2 }],
  "origen_canal": "BOOKING",
  "cliente_invitado": {
    "tipo_identificacion": "CEDULA",
    "numero_identificacion": "1712345678",
    "nombres": "Juan Carlos",
    "apellidos": "Pérez Gómez",
    "correo": "juan.perez@email.com",
    "telefono": "0991234567",
    "direccion": "Av. Principal 123"
  }
}
```

**201 Created** — `rev_estado`: `PENDIENTE`, `_links.confirmar_pago`.

**Errores:** `400`, `401` (JWT inválido si se envía), `404`, `409`, `500`.

---

### Endpoint 8 — Listar mis reservas

`GET /api/v2/reservas`

**Auth:** JWT obligatorio. Query: `page`, `limit`.

**200 OK** — `rev_estado` ej. `PAGADA`.

**Errores:** `401`, `500`.

---

### Endpoint 9 — Detalle reserva

`GET /api/v2/reservas/{guid}`

**Auth:** JWT obligatorio.

**Errores:** `401`, `403`, `404`, `500`.

---

### Endpoint 10 — Confirmar pago

`POST /api/v2/reservas/{guid}/pagos/confirmacion`

**Auth:** no requerida.

**Body:**

```json
{
  "nombre_receptor": "Juan Carlos",
  "apellido_receptor": "Pérez Gómez",
  "correo_receptor": "juan@email.com",
  "telefono_receptor": "0991234567",
  "observacion": "string"
}
```

**201 Created** — `fac_numero`, `estado`: `E`, etc.

**Errores:** `400`, `404`, `409`, `500`.

---

## 4) Resumen integración (10 endpoints)

| # | Endpoint | Método |
|---|----------|--------|
| 1 | `/api/v2/atracciones` | GET |
| 2 | `/api/v2/atracciones/filtros` | GET |
| 3 | `/api/v2/atracciones/{guid}` | GET |
| 4 | `/api/v2/atracciones/{guid}/tickets` | GET |
| 5 | `/api/v2/atracciones/{guid}/horarios` | GET |
| 6 | `/api/v2/atracciones/{guid}/horarios/{horarioGuid}/tickets` | GET |
| 7 | `/api/v2/reservas` | POST |
| 8 | `/api/v2/reservas` | GET |
| 9 | `/api/v2/reservas/{guid}` | GET |
| 10 | `/api/v2/reservas/{guid}/pagos/confirmacion` | POST |

---

## 5) Códigos HTTP globales

| Código | Significado | Body |
|--------|-------------|------|
| 200 | OK | Sí |
| 201 | Created | Sí |
| 204 | No Content | No |
| 400 | Bad Request | Sí (Error) |
| 401 | Unauthorized | Sí (Error) |
| 403 | Forbidden | Sí (Error) |
| 404 | Not Found | Sí (Error) |
| 409 | Conflict | Sí (Error) |
| 500 | Internal Server Error | Sí (Error) |

---

## 6) Estructura de errores

```json
{
  "status": 400,
  "error": "Parámetro inválido",
  "details": ["El campo 'limit' debe ser un entero entre 1 y 50."],
  "timestamp": "2026-05-19T14:30:00Z",
  "path": "/api/v2/atracciones"
}
```

---

## 7) Notas de implementación

- **Saga:** `POST /reservas` reserva cupo (ms-atracciones) y persiste reserva pendiente (ms-reservas) vía **ms-orquestador**.
- **Invitado:** CRM alta/reutilización por `numero_identificacion` (gRPC `ClienteService`).
- **Confirmación:** confirma ventas + emite factura (ms-facturación); auditoría best-effort.
- **PayPal (anexo):** `POST /api/v2/pagos/paypal/orders` + `paypal_order_id` en confirmación.
- **Idempotency-Key:** recomendada en POST; almacenada en BD del orquestador si se envía.

---

## 8) Anexo — rutas internas (no PDF Booking)

| Método | Ruta | Auth |
|--------|------|------|
| POST | `/api/v2/auth/login` | No |
| POST | `/api/v2/auth/registro` | No |
| PUT | `/api/v2/reservas/{guid}/cancelar` | JWT |
| POST | `/api/v2/pagos/paypal/orders` | JWT |
| GET | `/api/v2/facturas/mis-facturas` | JWT |
| GET/POST | `/api/v2/atracciones/{guid}/resenias` | GET no / POST JWT |

**No usar en contrato Booking:** `horarios-disponibles` (usar `/horarios`), `confirmar-pago` (usar `/pagos/confirmacion`).

---

## 9) Matriz HTTP por endpoint

| Endpoint | 200/201 | 400 | 401 | 403 | 404 | 409 | 500 |
|----------|---------|-----|-----|-----|-----|-----|-----|
| GET /atracciones | 200 | ✓ | | | | | ✓ |
| GET /atracciones/filtros | 200 | ✓ | | | | | ✓ |
| GET /atracciones/{guid} | 200 | | | | ✓ | | ✓ |
| GET /atracciones/{guid}/tickets | 200 | | | | ✓ | | ✓ |
| GET /atracciones/{guid}/horarios | 200 | | | | ✓ | | ✓ |
| GET .../horarios/{horarioGuid}/tickets | 200 | | | | ✓ | | ✓ |
| POST /reservas | 201 | ✓ | ✓* | | ✓ | ✓ | ✓ |
| GET /reservas | 200 | | ✓ | | | | ✓ |
| GET /reservas/{guid} | 200 | | ✓ | ✓ | ✓ | | ✓ |
| POST .../pagos/confirmacion | 201 | ✓ | | | ✓ | ✓ | ✓ |

\*401 solo si se envía JWT inválido.

---

## 10) OpenAPI 3.0.3

```yaml
openapi: 3.0.3
info:
  title: API de Atracciones - Booking Público
  version: 2.0.0
  description: Contrato v2 para integración Booking (10 endpoints principales).
servers:
  - url: https://api-gateway-production-5c80b.up.railway.app/api/v2
    description: Producción
  - url: http://localhost:5050/api/v2
    description: Desarrollo Docker

paths:
  /atracciones:
    get:
      operationId: listarAtracciones
      tags: [Catálogo]
      parameters:
        - { name: ciudad, in: query, schema: { type: string } }
        - { name: tipo, in: query, schema: { type: string } }
        - { name: subtipo, in: query, schema: { type: string } }
        - { name: etiqueta, in: query, schema: { type: string, enum: [free_cancellation, skip_the_line] } }
        - { name: page, in: query, schema: { type: integer, default: 1, minimum: 1 } }
        - { name: limit, in: query, schema: { type: integer, default: 10, minimum: 1, maximum: 50 } }
      responses:
        "200":
          description: Listado paginado
          content:
            application/json:
              schema: { $ref: "#/components/schemas/ListadoAtraccionesEnvelope" }
        "400": { $ref: "#/components/responses/BadRequest" }
        "500": { $ref: "#/components/responses/InternalError" }

  /atracciones/filtros:
    get:
      operationId: obtenerFiltros
      tags: [Catálogo]
      parameters:
        - { name: ciudad, in: query, schema: { type: string } }
      responses:
        "200":
          description: Filtros del buscador
          content:
            application/json:
              schema: { $ref: "#/components/schemas/FiltrosEnvelope" }
        "400": { $ref: "#/components/responses/BadRequest" }
        "500": { $ref: "#/components/responses/InternalError" }

  /atracciones/{guid}:
    get:
      operationId: detalleAtraccion
      tags: [Catálogo]
      parameters:
        - { name: guid, in: path, required: true, schema: { type: string, format: uuid } }
      responses:
        "200": { description: Detalle, content: { application/json: { schema: { $ref: "#/components/schemas/ItemEnvelope" } } } }
        "404": { $ref: "#/components/responses/NotFound" }
        "500": { $ref: "#/components/responses/InternalError" }

  /atracciones/{guid}/tickets:
    get:
      operationId: ticketsAtraccion
      tags: [Catálogo]
      parameters:
        - { name: guid, in: path, required: true, schema: { type: string, format: uuid } }
      responses:
        "200":
          description: Tipos de ticket
          content:
            application/json:
              schema:
                type: object
                properties:
                  status: { type: integer, example: 200 }
                  data:
                    type: array
                    items: { $ref: "#/components/schemas/TicketSimple" }
        "404": { $ref: "#/components/responses/NotFound" }

  /atracciones/{guid}/horarios:
    get:
      operationId: horariosAtraccion
      tags: [Catálogo]
      parameters:
        - { name: guid, in: path, required: true, schema: { type: string, format: uuid } }
        - { name: disponibles, in: query, schema: { type: boolean, default: true } }
      responses:
        "200":
          description: Horarios
          content:
            application/json:
              schema:
                type: object
                properties:
                  status: { type: integer }
                  data:
                    type: array
                    items: { $ref: "#/components/schemas/HorarioSimple" }

  /atracciones/{guid}/horarios/{horarioGuid}/tickets:
    get:
      operationId: ticketsPorHorario
      tags: [Catálogo]
      parameters:
        - { name: guid, in: path, required: true, schema: { type: string, format: uuid } }
        - { name: horarioGuid, in: path, required: true, schema: { type: string, format: uuid } }
      responses:
        "200":
          description: Tickets del slot
          content:
            application/json:
              schema: { $ref: "#/components/schemas/TicketsPorHorarioEnvelope" }

  /reservas:
    post:
      operationId: crearReserva
      tags: [Reservas]
      parameters:
        - name: Idempotency-Key
          in: header
          required: false
          schema: { type: string, format: uuid }
      requestBody:
        required: true
        content:
          application/json:
            schema: { $ref: "#/components/schemas/CrearReservaRequest" }
      responses:
        "201":
          description: Reserva PENDIENTE
          content:
            application/json:
              schema: { $ref: "#/components/schemas/ReservaCreadaEnvelope" }
        "400": { $ref: "#/components/responses/BadRequest" }
        "404": { $ref: "#/components/responses/NotFound" }
        "409": { $ref: "#/components/responses/Conflict" }
    get:
      operationId: listarMisReservas
      tags: [Reservas]
      security: [{ bearerAuth: [] }]
      parameters:
        - { name: page, in: query, schema: { type: integer, default: 1 } }
        - { name: limit, in: query, schema: { type: integer, default: 10 } }
      responses:
        "200": { description: Listado }
        "401": { $ref: "#/components/responses/Unauthorized" }

  /reservas/{guid}:
    get:
      operationId: obtenerReserva
      tags: [Reservas]
      security: [{ bearerAuth: [] }]
      parameters:
        - { name: guid, in: path, required: true, schema: { type: string, format: uuid } }
      responses:
        "200": { description: Detalle reserva }
        "401": { $ref: "#/components/responses/Unauthorized" }
        "403": { $ref: "#/components/responses/Forbidden" }
        "404": { $ref: "#/components/responses/NotFound" }

  /reservas/{guid}/pagos/confirmacion:
    post:
      operationId: confirmarPago
      tags: [Reservas]
      parameters:
        - { name: guid, in: path, required: true, schema: { type: string, format: uuid } }
        - name: Idempotency-Key
          in: header
          required: false
          schema: { type: string, format: uuid }
      requestBody:
        required: true
        content:
          application/json:
            schema: { $ref: "#/components/schemas/ConfirmarPagoRequest" }
      responses:
        "201":
          description: Factura emitida
          content:
            application/json:
              schema: { $ref: "#/components/schemas/FacturaEnvelope" }
        "400": { $ref: "#/components/responses/BadRequest" }
        "404": { $ref: "#/components/responses/NotFound" }
        "409": { $ref: "#/components/responses/Conflict" }

components:
  securitySchemes:
    bearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT

  schemas:
    TicketSimple:
      type: object
      properties:
        tck_guid: { type: string, format: uuid }
        tipo: { type: string }
        precio: { type: number }
        moneda: { type: string, example: USD }
    HorarioSimple:
      type: object
      properties:
        hor_guid: { type: string, format: uuid }
        fecha: { type: string, format: date }
        hora_inicio: { type: string }
        hora_fin: { type: string }
        cupos: { type: integer }
    ClienteInvitado:
      type: object
      required: [tipo_identificacion, numero_identificacion, correo]
      properties:
        tipo_identificacion: { type: string, maxLength: 20 }
        numero_identificacion: { type: string, maxLength: 20 }
        nombres: { type: string }
        apellidos: { type: string }
        correo: { type: string, format: email }
        telefono: { type: string }
        direccion: { type: string }
    CrearReservaRequest:
      type: object
      required: [at_guid, hor_guid, lineas]
      properties:
        at_guid: { type: string, format: uuid }
        hor_guid: { type: string, format: uuid }
        fecha_visita: { type: string, format: date }
        lineas:
          type: array
          minItems: 1
          items:
            type: object
            required: [tck_guid, cantidad]
            properties:
              tck_guid: { type: string, format: uuid }
              cantidad: { type: integer, minimum: 1 }
        origen_canal: { type: string, example: BOOKING }
        cliente_invitado: { $ref: "#/components/schemas/ClienteInvitado" }
    ConfirmarPagoRequest:
      type: object
      required: [nombre_receptor, correo_receptor]
      properties:
        nombre_receptor: { type: string }
        apellido_receptor: { type: string }
        correo_receptor: { type: string, format: email }
        telefono_receptor: { type: string }
        observacion: { type: string }
        paypal_order_id: { type: string }
    TicketsPorHorarioEnvelope:
      type: object
      properties:
        status: { type: integer, example: 200 }
        message: { type: string }
        data:
          type: object
          properties:
            items:
              type: array
              items: { $ref: "#/components/schemas/TicketSimple" }
    Error:
      type: object
      properties:
        status: { type: integer }
        error: { type: string }
        details: { type: array, items: { type: string } }
        timestamp: { type: string, format: date-time }
        path: { type: string }

  responses:
    BadRequest:
      description: Bad Request
      content:
        application/json:
          schema: { $ref: "#/components/schemas/Error" }
    Unauthorized:
      description: Unauthorized
    Forbidden:
      description: Forbidden
    NotFound:
      description: Not Found
    Conflict:
      description: Conflict
    InternalError:
      description: Internal Server Error
```
