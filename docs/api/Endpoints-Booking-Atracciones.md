# Documentación de Endpoints para Booking Externo — Sistema Atracciones

Contrato de integración pública **v2.0.0** (base `/api/v2`). Referencia normativa: [`Contrato_API_Atracciones_V2.pdf`](Contrato_API_Atracciones_V2.pdf). Especificación extendida: [`MicroservicioAtracionesAPI/docs/api/openapi-v2-booking-public.md`](../../MicroservicioAtracionesAPI/docs/api/openapi-v2-booking-public.md).

Todas las solicitudes deben hacerse al **API Gateway** (no llamar microservicios internos directamente).

---

## Base URL

| Entorno | URL base |
|---------|----------|
| Producción (Railway) | `https://api-gateway-production-5c80b.up.railway.app/api/v2` |
| Docker Compose (Windows) | `http://localhost:5050/api/v2` |
| Gateway `dotnet run` | `http://localhost:5000/api/v2` |

**Health:** `GET https://api-gateway-production-5c80b.up.railway.app/health`

---

## Convenciones técnicas

- **Dominio (negocio):** snake_case (`rev_guid`, `at_guid`, `nombre_receptor`).
- **Metadatos de listado/filtros:** camelCase (`filterStats`, `destinationFilters`, `defaultSorter`).
- **Envelope:** `{ "status", "message", "data", "pagination?" }`.
- **Errores:** `{ "status", "error", "details", "timestamp", "path" }`.
- **Moneda:** USD.
- **Estados de reserva (respuesta pública):** `PENDIENTE`, `PAGADA`, `CANCELADA`, `INACTIVA`.
- **Paginación:** `page` (≥ 1), `limit` (1–50).
- **Idempotencia:** cabecera `Idempotency-Key` (UUID) **recomendada** en `POST /reservas` y `POST .../pagos/confirmacion` (opcional en implementación v2).
- **Correlación:** cabecera opcional `X-Correlation-ID`.

---

## Contrato Booking — 10 endpoints

| # | Método | Ruta | Auth |
|---|--------|------|------|
| 1 | GET | `/api/v2/atracciones` | No |
| 2 | GET | `/api/v2/atracciones/filtros` | No |
| 3 | GET | `/api/v2/atracciones/{guid}` | No |
| 4 | GET | `/api/v2/atracciones/{guid}/tickets` | No |
| 5 | GET | `/api/v2/atracciones/{guid}/horarios` | No |
| 6 | GET | `/api/v2/atracciones/{guid}/horarios/{horarioGuid}/tickets` | No |
| 7 | POST | `/api/v2/reservas` | Opcional (ver abajo) |
| 8 | GET | `/api/v2/reservas` | JWT |
| 9 | GET | `/api/v2/reservas/{guid}` | JWT |
| 10 | POST | `/api/v2/reservas/{guid}/pagos/confirmacion` | No |

---

## Autenticación

### POST /reservas (crear)

- **Sin JWT:** enviar objeto `cliente_invitado` en el body (obligatorio `tipo_identificacion`, `numero_identificacion`, `correo`).
- **Con JWT:** `Authorization: Bearer {token}`; `cliente_invitado` se ignora; `cli_guid` = claim `usu_guid`.

### POST /pagos/confirmacion

- **No requiere** Bearer. El `guid` de la reserva identifica la operación.

### GET /reservas y GET /reservas/{guid}

- **JWT obligatorio.** Solo reservas del cliente del token.

---

## 1–6. Catálogo (ms-atracciones)

### GET /atracciones

Query opcionales: `ciudad`, `tipo`, `subtipo`, `etiqueta`, `idioma`, `calificacion_min`, `horario`, `disponible`, `ordenar_por`, `page`, `limit`.

Respuesta 200 (extracto):

```json
{
  "status": 200,
  "message": "Consulta exitosa",
  "data": [
    {
      "id": "uuid",
      "nombre": "Tour Quito",
      "ciudad": "Quito",
      "pais": "Ecuador",
      "precio_desde": 25.0,
      "moneda": "USD",
      "calificacion": 4.5,
      "disponibilidad": { "disponible": true, "cupos_disponibles": 12 },
      "_links": { "self": "/api/v2/atracciones/{guid}" }
    }
  ],
  "pagination": { "page": 1, "limit": 10, "total": 85, "total_pages": 9 },
  "filterStats": { "filteredProductCount": 85, "unfilteredProductCount": 210 },
  "sorters": [{ "name": "Más populares", "value": "trending" }],
  "defaultSorter": { "name": "Más populares", "value": "trending" }
}
```

### GET /atracciones/filtros

Query opcional: `ciudad`. Respuesta: `data.destinationFilters`, `typeFilters`, `labelFilters`, etc.

### GET /atracciones/{guid}

Detalle completo: descripción, imágenes, incluye, tickets, `horarios_proximos`.

### GET /atracciones/{guid}/tickets

```json
{
  "status": 200,
  "data": [
    { "tck_guid": "uuid", "tipo": "Adulto", "precio": 25.0, "moneda": "USD" }
  ]
}
```

### GET /atracciones/{guid}/horarios

Por defecto solo horarios con cupo (`disponibles=true`). Query opcional: `disponibles=false` para ver todos.

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

### GET /atracciones/{guid}/horarios/{horarioGuid}/tickets

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

---

## 7–10. Reservas (ms-orquestador)

### POST /reservas

**Body (sin JWT — Booking):**

```json
{
  "at_guid": "40000000-0000-0000-0000-000000000001",
  "hor_guid": "50000000-0000-0000-0000-000000000001",
  "lineas": [
    { "tck_guid": "60000000-0000-0000-0000-000000000001", "cantidad": 2 }
  ],
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

| Campo | Obligatorio | Notas |
|-------|-------------|-------|
| `at_guid` | Sí | Debe coincidir con la atracción del horario |
| `hor_guid` | Sí | Del endpoint `/horarios` |
| `lineas[].tck_guid` | Sí | |
| `lineas[].cantidad` | Sí | ≥ 1 |
| `origen_canal` | No | Enviar `BOOKING` |
| `cliente_invitado` | Cond. | Obligatorio sin JWT |
| `cliente_invitado.correo` | Sí* | |
| `cliente_invitado.tipo_identificacion` | Sí* | |
| `cliente_invitado.numero_identificacion` | Sí* | |

**Respuesta 201:**

```json
{
  "status": 201,
  "message": "Operación exitosa",
  "data": {
    "rev_guid": "uuid",
    "rev_codigo": "RES-2026-00123",
    "rev_estado": "PENDIENTE",
    "rev_total": 57.5,
    "moneda": "USD",
    "detalle": [{ "tck_tipo_participante": "Adulto", "cantidad": 2, "precio_unit": 25.0, "subtotal": 50.0 }],
    "_links": {
      "self": "/api/v2/reservas/{rev_guid}",
      "confirmar_pago": "/api/v2/reservas/{rev_guid}/pagos/confirmacion"
    }
  }
}
```

Códigos: `400`, `401` (si JWT inválido), `404`, `409` (cupos), `500`.

### GET /reservas

JWT. Query: `page`, `limit`. Listado con `rev_estado` (`PAGADA`, etc.).

### GET /reservas/{guid}

JWT. Detalle completo de la reserva del cliente.

### POST /reservas/{guid}/pagos/confirmacion

**Sin Authorization.**

```json
{
  "nombre_receptor": "Juan Carlos",
  "apellido_receptor": "Pérez Gómez",
  "correo_receptor": "juan@email.com",
  "telefono_receptor": "0991234567",
  "observacion": "Pago Booking"
}
```

| Campo | Obligatorio |
|-------|-------------|
| `nombre_receptor` | Sí |
| `correo_receptor` | Sí |
| `apellido_receptor` | No |
| `telefono_receptor` | No |
| `paypal_order_id` | No (si se usó PayPal) |

**Respuesta 201:**

```json
{
  "status": 201,
  "message": "Operación exitosa",
  "data": {
    "fac_guid": "uuid",
    "fac_numero": "FAC-2026-00456",
    "rev_codigo": "RES-2026-00123",
    "total": 57.5,
    "moneda": "USD",
    "estado": "E",
    "nombre_receptor": "Juan Carlos",
    "correo_receptor": "juan@email.com"
  }
}
```

---

## Anexo — Ecosistema interno (fuera del PDF Booking)

| Método | Ruta | Auth |
|--------|------|------|
| POST | `/api/v2/auth/login` | No |
| POST | `/api/v2/auth/registro` | No |
| PUT | `/api/v2/reservas/{guid}/cancelar` | JWT |
| POST | `/api/v2/pagos/paypal/orders` | JWT |
| GET | `/api/v2/facturas/mis-facturas` | JWT |
| GET/POST | `/api/v2/atracciones/{guid}/resenias` | GET no / POST JWT |
| GET | `/api/v2/tickets/{guid}/horarios` | No |

---

## Flujo Booking recomendado

1. `GET /atracciones/filtros` y `GET /atracciones`
2. `GET /atracciones/{guid}`
3. `GET /atracciones/{guid}/horarios`
4. `GET /atracciones/{guid}/horarios/{horarioGuid}/tickets` → usar `data.items`
5. `POST /reservas` con `cliente_invitado` (sin JWT)
6. `POST /reservas/{rev_guid}/pagos/confirmacion` (sin JWT)

---

## Códigos HTTP

| Código | Uso |
|--------|-----|
| 200 | Consulta OK |
| 201 | Reserva o factura creada |
| 204 | Cancelación sin body |
| 400 | Parámetros/body inválidos |
| 401 | Token ausente o inválido |
| 403 | Reserva de otro cliente |
| 404 | GUID inexistente |
| 409 | Cupos, estado inválido |
| 500 | Error interno |

---

Versión: mayo 2026 — API v2 única.
