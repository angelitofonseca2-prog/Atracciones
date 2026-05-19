# Documentación de Endpoints para Booking Externo — Sistema Atracciones

Este documento describe los endpoints públicos del ecosistema **Atracciones** (monorepo con API Gateway YARP, microservicios y orquestador de sagas), tal como están implementados en el repositorio. Está alineado con `MicroservicioAtracionesAPI/docs/api/openapi-v2-booking-public.md`.

Todas las solicitudes de integración deben hacerse a través del **API Gateway**. No consumir directamente los microservicios internos (`ms-atracciones`, `ms-orquestador`, etc.).

---

## Base URL

**Producción (Railway)**

https://api-gateway-production-5c80b.up.railway.app

Prefijo API: `/api/v1`

URL base completa: `https://api-gateway-production-5c80b.up.railway.app/api/v1`

**Frontend web (referencia)**

https://frontend-atracciones-production.up.railway.app

**Desarrollo local (Docker Compose en Windows)**

http://localhost:5050/api/v1

**Desarrollo local (gateway con dotnet run)**

http://localhost:5000/api/v1

**Comprobación de salud del gateway**

GET https://api-gateway-production-5c80b.up.railway.app/health

---

## Convenciones técnicas

- **Formato JSON:** snake_case en propiedades (`at_guid`, `rev_guid`, `origen_canal`).
- **Envelope de respuesta:** `{ "status", "message", "data", "pagination?" }`.
- **Errores:** `{ "status", "error", "details", "path" }`.
- **Moneda por defecto:** USD.
- **Estados de reserva:** `P` = pendiente de pago, `A` = confirmada (pagada), `C` = cancelada, `I` = inactiva.
- **Idempotencia:** cabecera obligatoria `Idempotency-Key` (UUID) en `POST /reservas` y `POST /reservas/{guid}/pagos/confirmacion`.
- **Correlación:** cabecera opcional `X-Correlation-ID` (UUID); si no se envía, el gateway puede generarla.
- **Paginación en listados:** `page` (≥ 1), `limit` (1–50).

---

## Autenticación

**Regla general:** toda operación de **reserva, pago, facturas del cliente y reseñas** exige que el usuario esté **registrado e iniciado sesión** (JWT). No existe flujo de `cliente_invitado` ni reserva anónima.

Los endpoints de **catálogo** (GET de atracciones, tickets, horarios, listado público de reseñas) no requieren token.

### Registro e inicio de sesión

**Login**

POST /api/v1/auth/login

Body: `{ "login": "usuario@ejemplo.com", "password": "********" }`

Respuesta 200: JWT RS256. Usar en todas las rutas protegidas:

`Authorization: Bearer {token}`

**Registro (saga orquestada)**

POST /api/v1/auth/registro

Crea usuario en `ms-identidad`, perfil CRM en `ms-reservas` y devuelve token para continuar con la reserva.

### Rutas que requieren JWT de cliente (`ClienteAutenticado`)

- POST /api/v1/reservas
- POST /api/v1/reservas/{guid}/pagos/confirmacion
- POST /api/v1/pagos/paypal/orders
- GET /api/v1/reservas (mis reservas)
- GET /api/v1/reservas/{guid}
- PUT /api/v1/reservas/{guid}/cancelar
- GET /api/v1/facturas/mis-facturas
- POST /api/v1/atracciones/{guid}/resenias
- GET /api/v1/clientes/perfil
- PUT /api/v1/clientes/perfil

El claim `usu_guid` del token debe coincidir con `cli_guid` en CRM (mismo GUID al registrarse).

---

## Catálogo de atracciones

Servicio detrás del gateway: **ms-atracciones**.

### Listar atracciones

GET /api/v1/atracciones

Requiere token: No.

Query opcionales: ciudad, tipo, subtipo, etiqueta, page, limit.

Respuesta 200: lista en `data` con atracciones activas; paginación en `pagination` si aplica.

### Obtener filtros de búsqueda

GET /api/v1/atracciones/filtros

Requiere token: No.

Respuesta 200: `destination_filters`, `type_filters`, `supported_language_filters`, `label_filters`, `time_of_day_filters`.

### Obtener detalle de atracción

GET /api/v1/atracciones/{guid}

Requiere token: No.

404 si el GUID no existe o no está disponible.

### Listar tickets de una atracción

GET /api/v1/atracciones/{guid}/tickets

Requiere token: No.

### Listar horarios de una atracción

GET /api/v1/atracciones/{guid}/horarios

Query: `disponibles=true` (recomendado; solo horarios con cupo).

Requiere token: No.

Legacy (evitar): GET /api/v1/atracciones/{guid}/horarios-disponibles — usar `horarios?disponibles=true`.

### Listar tickets disponibles para un horario

GET /api/v1/atracciones/{guid}/horarios/{horario_guid}/tickets

Requiere token: No.

404 si el horario no existe o no hay disponibilidad.

### Horarios por ticket (auxiliar)

GET /api/v1/tickets/{guid}/horarios

Requiere token: No.

---

## Reservas y pagos

Servicio detrás del gateway: **ms-orquestador** (saga síncrona: inventario + ventas + facturación).

### Flujo recomendado

1. POST /api/v1/reservas → reserva en estado `P` (pendiente), cupo reservado.
2. (Opcional) POST /api/v1/pagos/paypal/orders → orden PayPal para `rev_guid`.
3. POST /api/v1/reservas/{rev_guid}/pagos/confirmacion → confirma pago, emite factura, estado `A`.

### Crear reserva

POST /api/v1/reservas

Cabeceras obligatorias:

- Content-Type: application/json
- Idempotency-Key: {uuid}
- Authorization: Bearer {token}

Body (snake_case):

{
  "at_guid": "40000000-0000-0000-0000-000000000001",
  "hor_guid": "50000000-0000-0000-0000-000000000001",
  "fecha_visita": "2026-05-20",
  "lineas": [
    { "tck_guid": "60000000-0000-0000-0000-000000000001", "cantidad": 2 }
  ],
  "origen_canal": "web"
}

Notas:

- `origen_canal`: `web` en la aplicación propia; integradores pueden usar `BOOKING` si aplica.
- Respuesta 201: `data` incluye `rev_guid`, `rev_estado` (`P`), totales (`rev_subtotal`, `rev_valor_iva`, `rev_total`), `rev_codigo`, enlaces HATEOAS en `_links` si aplica.
- 409 si no hay cupos o regla de negocio (reserva duplicada pendiente, etc.).

### Consultar reserva (cliente autenticado)

GET /api/v1/reservas/{guid}

Authorization: Bearer {token}

Solo el cliente dueño de la reserva. 403 si no pertenece al usuario del token.

### Listar mis reservas

GET /api/v1/reservas?page=1&limit=10

Authorization: Bearer {token}

### Confirmar pago

POST /api/v1/reservas/{guid}/pagos/confirmacion

Cabeceras:

- Content-Type: application/json
- Idempotency-Key: {uuid}

Body:

{
  "nombre_receptor": "Booking",
  "apellido_receptor": "Tester",
  "correo_receptor": "booking.tester@example.com",
  "telefono_receptor": "0999999999",
  "observacion": "Pago confirmado desde Booking",
  "paypal_order_id": "ORDEN_PAYPAL_OPCIONAL"
}

Notas:

- Si se envía `paypal_order_id`, se valida/captura con PayPal antes de completar la saga.
- Si se omite `paypal_order_id`, confirma pago sin pasarela (solo entornos controlados / pruebas).
- Respuesta 201: datos de factura en `data` (`fac_guid`, `fac_numero`, `rev_guid`, `total`, etc.).

### Crear orden PayPal (auxiliar)

POST /api/v1/pagos/paypal/orders

Body: incluye `rev_guid` de reserva pendiente existente (y datos según contrato del controlador PayPal).

### Cancelar reserva (cliente autenticado)

PUT /api/v1/reservas/{guid}/cancelar

Authorization: Bearer {token}

Body: { "motivo": "Cancelada por el cliente" }

Respuesta: 204. Libera cupo en inventario vía orquestador.

### Legacy

POST /api/v1/reservas/{guid}/confirmar-pago — alias obsoleto de `pagos/confirmacion`.

---

## Facturación

Servicio: **ms-facturacion** (lectura REST vía gateway).

### Mis facturas (cliente autenticado)

GET /api/v1/facturas/mis-facturas?page=1&limit=10

Authorization: Bearer {token}

No existe en este proyecto `GET /api/v1/booking/facturas?reservaGuid=...`. Para integradores: usar la respuesta de confirmación de pago (`fac_guid`, `fac_numero`) o consultar con JWT del cliente en `mis-facturas`.

---

## Reseñas

Servicio: **ms-atracciones**.

### Listar reseñas públicas

GET /api/v1/atracciones/{guid}/resenias?page=1&page_size=10

Requiere token: No.

### Crear reseña

POST /api/v1/atracciones/{guid}/resenias

Authorization: Bearer {token} (cliente autenticado).

Body:

{
  "rev_guid": "GUID_RESERVA_CONFIRMADA",
  "rating": 5,
  "comentario": "Excelente experiencia"
}

Solo reservas en estado confirmado (`A`). El `guid` de la atracción va en la URL.

---

## Códigos de error HTTP

| Código | Significado | Cuándo |
|--------|-------------|--------|
| 200 | OK | Consulta exitosa |
| 201 | Created | Reserva o confirmación de pago creada |
| 204 | No Content | Cancelación admin/cliente sin body |
| 400 | Bad Request | Body inválido, falta Idempotency-Key |
| 401 | Unauthorized | Falta o token inválido |
| 403 | Forbidden | Recurso de otro usuario |
| 404 | Not Found | GUID inexistente |
| 409 | Conflict | Cupos, estado de reserva, idempotencia |
| 500 | Internal Server Error | Error no controlado |
| 502 | Bad Gateway | Fallo entre gateway y microservicio |

---

## Flujo recomendado (web o integrador con cuenta)

1. POST /api/v1/auth/registro o POST /api/v1/auth/login → obtener JWT
2. GET /api/v1/atracciones/filtros
3. GET /api/v1/atracciones
4. GET /api/v1/atracciones/{guid}
5. GET /api/v1/atracciones/{guid}/horarios?disponibles=true
6. GET /api/v1/atracciones/{guid}/horarios/{horario_guid}/tickets
7. POST /api/v1/reservas (JWT + Idempotency-Key)
8. (Opcional) POST /api/v1/pagos/paypal/orders
9. POST /api/v1/reservas/{rev_guid}/pagos/confirmacion (JWT + Idempotency-Key)
10. GET /api/v1/facturas/mis-facturas o usar `fac_guid` de la respuesta de confirmación

---

## Consideraciones generales

- Consumir siempre por **API Gateway** (URLs de Railway anteriores).
- El **rev_guid** es la llave de integración para reserva, pago y factura.
- **Toda reserva exige usuario registrado** con perfil CRM (`cli_guid` = `usu_guid` del token).
- JSON en **snake_case**.
- Estados de reserva como **un carácter**: `P`, `A`, `C`, `I`.
- Microservicios desplegados: ms-identidad, ms-atracciones (catálogo + inventario), ms-reservas (CRM + ventas), ms-orquestador, ms-facturacion, ms-auditoria.
- Documentación OpenAPI de referencia en el repositorio: `MicroservicioAtracionesAPI/docs/api/openapi-v2-booking-public.md`.

---

## Endpoints de administración (fuera del contrato Booking público)

Requieren rol Admin (JWT). Ejemplos:

- GET/PUT/DELETE /api/v1/admin/reservas
- CRUD /api/v1/admin/atracciones, horarios, tickets, destinos, categorías
- POST /api/v1/admin/auth/login

No deben usarse por el integrador Booking salvo acuerdo operativo explícito.

---

Documento generado para el proyecto Atracciones — arquitectura Strangler Fig / microservicios .NET 10.

Versión: alineada al repositorio (mayo 2026).
