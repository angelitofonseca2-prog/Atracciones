# Contrato de Integración API de Atracciones v2.0 (Booking Público)

## 1) Objetivo y alcance

Este documento formaliza el contrato de integración pública para Booking sobre la API v1 actual del microservicio:

- `GET /api/v1/atracciones`
- `GET /api/v1/atracciones/{guid}`
- `GET /api/v1/atracciones/{guid}/tickets`
- `GET /api/v1/atracciones/{guid}/horarios-disponibles`
- `GET /api/v1/atracciones/filtros`
- `GET /api/v1/reservas` (Mis Reservas)
- `POST /api/v1/reservas`
- `GET /api/v1/reservas/{guid}`
- `PUT /api/v1/reservas/{guid}/cancelar`
- `POST /api/v1/reservas/{guid}/confirmar-pago`
- `GET /api/v1/facturas/mis-facturas`
- `GET /api/v1/tickets/{guid}/horarios`

Incluye reglas funcionales, códigos HTTP, estructura de errores y contrato OpenAPI 3.0.3 actualizado con reservas.

## 2) Reglas funcionales y técnicas

- Los endpoints `GET` son idempotentes y no alteran estado.
- Listados vacíos retornan `200 OK` con `data: []` (no `404`).
- `404 Not Found` se reserva para recursos por GUID inexistentes/inactivos.
- Paginación obligatoria en listados (`page >= 1`, `1 <= limit <= 50`).
- Moneda por defecto en respuestas públicas: `USD`.
- URLs de imágenes deben ser absolutas y `https`.
- Campos opcionales ausentes se devuelven como `null` (no se omiten).
- HATEOAS: entidades principales incluyen `_links`.
- Disponibilidad en tiempo real (sin caché en ese objeto).
- En respuestas con envelope (`status`, `message`, `data`), el campo `status` del JSON coincide con el código HTTP de la respuesta (por ejemplo `201` en `POST /reservas` cuando la creación es exitosa).

## 3) Políticas de caché recomendadas

| Endpoint | Cacheable | TTL | Observación |
|---|---|---|---|
| `/atracciones` | Sí | 5 min | Metadatos cambian con baja frecuencia |
| `/atracciones/{guid}` | Sí | 5 min | Detalle estable en periodos cortos |
| `disponibilidad` (objeto interno) | No | Sin caché | Requiere cupos reales de `HORARIO` |
| `/atracciones/filtros` | Sí | 6 horas | Catálogos relativamente estables |
| `/reservas` | No | Sin caché | Operación transaccional de escritura |
| `/reservas/{guid}` | No | Sin caché | Estado y trazabilidad de compra |

## 4) Códigos HTTP

| Código | Significado | Cuándo ocurre | Body |
|---|---|---|---|
| `200` | OK | Consulta exitosa (incluye `data: []`) | Sí |
| `201` | Created | Reserva creada exitosamente | Sí |
| `400` | Bad Request | Parámetros/body inválidos | Sí (`Error`) |
| `401` | Unauthorized | Falta token o token inválido | Sí (`Error`) |
| `403` | Forbidden | Recurso no permitido para el cliente | Sí (`Error`) |
| `404` | Not Found | GUID no existe o no está disponible | Sí (`Error`) |
| `409` | Conflict | Regla de negocio (p. ej. cupos insuficientes) | Sí (`Error`) |
| `500` | Internal Server Error | Error no controlado | Sí (`Error`) |

## 5) Estructura uniforme de errores

```json
{
  "status": 400,
  "error": "Parámetro inválido",
  "details": [
    "El campo 'limit' debe ser un entero entre 1 y 50. Valor recibido: -1"
  ],
  "timestamp": "2025-07-01T14:30:00Z",
  "path": "/api/v1/atracciones"
}
```

## 6) OpenAPI 3.0.3 (contrato formal)

```yaml
openapi: 3.0.3
info:
  title: API de Atracciones - Booking Público v2
  version: 2.0.0
  contact:
    name: Equipo de Integración - Booking
servers:
  - url: https://api.misistema.com/api/v1
    description: Producción
  - url: http://localhost:5031/api/v1
    description: Desarrollo

paths:
  /atracciones:
    get:
      operationId: listarAtracciones
      tags: [Atracciones]
      parameters:
        - in: query
          name: ciudad
          schema: { type: string }
        - in: query
          name: tipo
          schema: { type: string }
          description: cat_guid raíz
        - in: query
          name: subtipo
          schema: { type: string }
          description: cat_guid hijo
        - in: query
          name: etiqueta
          schema:
            type: string
            enum: [free_cancellation, skip_the_line]
        - in: query
          name: idioma
          schema:
            type: string
            enum: [en, es, fr, it, de, ru, pt, ja, ar, pl]
        - in: query
          name: calificacion_min
          schema:
            type: number
            enum: [3.0, 3.5, 4.0, 4.5]
        - in: query
          name: horario
          schema:
            type: string
            enum: ["05:00-12:00", "12:00-18:00", "18:00-05:00"]
        - in: query
          name: disponible
          schema: { type: boolean }
        - in: query
          name: ordenar_por
          schema:
            type: string
            enum: [trending, lowest_price, highest_weighted_rating]
            default: trending
        - in: query
          name: page
          schema:
            type: integer
            default: 1
            minimum: 1
        - in: query
          name: limit
          schema:
            type: integer
            default: 10
            minimum: 1
            maximum: 50
      responses:
        "200": { $ref: "#/components/responses/ListadoAtraccionesOK" }
        "400": { $ref: "#/components/responses/BadRequest" }
        "500": { $ref: "#/components/responses/InternalError" }

  /atracciones/{guid}:
    get:
      operationId: obtenerAtraccionPorGuid
      tags: [Atracciones]
      parameters:
        - in: path
          name: guid
          required: true
          schema:
            type: string
            format: uuid
      responses:
        "200": { $ref: "#/components/responses/DetalleAtraccionOK" }
        "404": { $ref: "#/components/responses/NotFound" }
        "500": { $ref: "#/components/responses/InternalError" }

  /atracciones/{guid}/tickets:
    get:
      operationId: listarTicketsAtraccion
      tags: [Atracciones]
      parameters:
        - in: path
          name: guid
          required: true
          schema:
            type: string
            format: uuid
      responses:
        "200":
          description: Listado de tickets para la atracción
          content:
            application/json:
              schema:
                type: object
                properties:
                  status: { type: integer, example: 200 }
                  data:
                    type: array
                    items: { $ref: "#/components/schemas/TicketDisponible" }
        "404": { $ref: "#/components/responses/NotFound" }
        "500": { $ref: "#/components/responses/InternalError" }

  /atracciones/{guid}/horarios-disponibles:
    get:
      operationId: listarHorariosDisponiblesAtraccion
      tags: [Atracciones]
      parameters:
        - in: path
          name: guid
          required: true
          schema:
            type: string
            format: uuid
      responses:
        "200":
          description: Listado de horarios con cupos para la atracción
          content:
            application/json:
              schema:
                type: object
                properties:
                  status: { type: integer, example: 200 }
                  data:
                    type: array
                    items: { $ref: "#/components/schemas/HorarioProximo" }
        "404": { $ref: "#/components/responses/NotFound" }
        "500": { $ref: "#/components/responses/InternalError" }

  /atracciones/filtros:
    get:
      operationId: obtenerFiltros
      tags: [Filtros]
      parameters:
        - in: query
          name: ciudad
          required: false
          schema: { type: string }
          description: Filtra los contadores por ciudad. Si se omite, retorna filtros globales.
      responses:
        "200": { $ref: "#/components/responses/FiltrosOK" }
        "400": { $ref: "#/components/responses/BadRequest" }
        "500": { $ref: "#/components/responses/InternalError" }

  /reservas:
    get:
      operationId: listarMisReservas
      tags: [Reservas]
      security:
        - bearerAuth: []
      parameters:
        - in: query
          name: page
          schema: { type: integer, default: 1 }
        - in: query
          name: limit
          schema: { type: integer, default: 10 }
      responses:
        "200":
          description: Listado de reservas del cliente
          content:
            application/json:
              schema:
                type: object
                properties:
                  status: { type: integer, example: 200 }
                  data:
                    type: array
                    items: { $ref: "#/components/schemas/Reserva" }
                  pagination: { $ref: "#/components/schemas/Paginacion" }
        "401": { $ref: "#/components/responses/Unauthorized" }
        "500": { $ref: "#/components/responses/InternalError" }
    post:
      operationId: crearReserva
      tags: [Reservas]
      description: |
        Crea una reserva. Acepta peticiones anónimas y autenticadas.
        - **Con JWT** (`ClienteAutenticado`): el `cli_id` se extrae del token; no se requiere `cliente_invitado`.
        - **Sin JWT** (invitado): se debe incluir el objeto `cliente_invitado` en el body.
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: "#/components/schemas/CrearReservaRequest"
      responses:
        "201": { $ref: "#/components/responses/ReservaCreada" }
        "400": { $ref: "#/components/responses/BadRequest" }
        "404": { $ref: "#/components/responses/NotFound" }
        "409": { $ref: "#/components/responses/Conflict" }
        "500": { $ref: "#/components/responses/InternalError" }

  /reservas/{guid}:
    get:
      operationId: obtenerReservaPorGuid
      tags: [Reservas]
      security:
        - bearerAuth: []
      parameters:
        - in: path
          name: guid
          required: true
          schema:
            type: string
            format: uuid
      responses:
        "200": { $ref: "#/components/responses/ReservaOK" }
        "401": { $ref: "#/components/responses/Unauthorized" }
        "403": { $ref: "#/components/responses/Forbidden" }
        "404": { $ref: "#/components/responses/NotFound" }
        "500": { $ref: "#/components/responses/InternalError" }

  /reservas/{guid}/confirmar-pago:
    post:
      operationId: confirmarPagoReserva
      tags: [Reservas]
      parameters:
        - in: path
          name: guid
          required: true
          schema:
            type: string
            format: uuid
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: "#/components/schemas/ConfirmarPagoReservaRequest"
      responses:
        "201": { $ref: "#/components/responses/FacturaItemOK" }
        "400": { $ref: "#/components/responses/BadRequest" }
        "404": { $ref: "#/components/responses/NotFound" }
        "409": { $ref: "#/components/responses/Conflict" }
        "500": { $ref: "#/components/responses/InternalError" }

  /reservas/{guid}/cancelar:
    put:
      operationId: cancelarReserva
      tags: [Reservas]
      security:
        - bearerAuth: []
      parameters:
        - in: path
          name: guid
          required: true
          schema:
            type: string
            format: uuid
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: "#/components/schemas/CancelarReservaRequest"
      responses:
        "204": { description: Sin contenido }
        "401": { $ref: "#/components/responses/Unauthorized" }
        "403": { $ref: "#/components/responses/Forbidden" }
        "404": { $ref: "#/components/responses/NotFound" }
        "409": { $ref: "#/components/responses/Conflict" }
        "500": { $ref: "#/components/responses/InternalError" }

  /facturas/mis-facturas:
    get:
      operationId: listarMisFacturas
      tags: [Facturas]
      security:
        - bearerAuth: []
      parameters:
        - in: query
          name: page
          schema: { type: integer, default: 1 }
        - in: query
          name: limit
          schema: { type: integer, default: 10 }
      responses:
        "200": { $ref: "#/components/responses/FacturaOK" }
        "401": { $ref: "#/components/responses/Unauthorized" }
        "500": { $ref: "#/components/responses/InternalError" }

  /tickets/{guid}/horarios:
    get:
      operationId: listarHorariosPorTicket
      tags: [Atracciones]
      parameters:
        - in: path
          name: guid
          required: true
          schema:
            type: string
            format: uuid
      responses:
        "200":
          description: Listado de horarios para el ticket
          content:
            application/json:
              schema:
                type: object
                properties:
                  status: { type: integer, example: 200 }
                  data:
                    type: array
                    items: { $ref: "#/components/schemas/HorarioProximo" }
        "404": { $ref: "#/components/responses/NotFound" }
        "500": { $ref: "#/components/responses/InternalError" }

components:
  securitySchemes:
    bearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT

  schemas:
    Disponibilidad:
      type: object
      properties:
        disponible: { type: boolean }
        disponible_hoy: { type: boolean }
        proxima_fecha_disponible:
          type: string
          format: date
          nullable: true
        cupos_disponibles:
          type: integer
          nullable: true

    TicketDisponible:
      type: object
      properties:
        tck_guid: { type: string, format: uuid }
        tipo: { type: string }
        precio: { type: number, format: double }
        moneda: { type: string, default: USD }

    HorarioProximo:
      type: object
      properties:
        fecha: { type: string, format: date }
        hora_inicio: { type: string }
        hora_fin: { type: string, nullable: true }
        cupos: { type: integer }

    AtraccionListado:
      type: object
      required:
        - id
        - nombre
        - ciudad
        - pais
        - tipo_tagname
        - tipo_nombre
        - descripcion_corta
        - precio_desde
        - moneda
        - calificacion
        - total_resenas
        - idiomas_disponibles
        - disponibilidad
        - _links
      properties:
        id: { type: string, format: uuid }
        nombre: { type: string }
        ciudad: { type: string }
        pais: { type: string }
        tipo_tagname: { type: string }
        tipo_nombre: { type: string }
        subtipo_tagname: { type: string, nullable: true }
        subtipo_nombre: { type: string, nullable: true }
        etiquetas:
          type: array
          items: { type: string }
        descripcion_corta:
          type: string
          maxLength: 150
        imagen_principal:
          type: string
          format: uri
          nullable: true
        duracion_minutos:
          type: integer
          nullable: true
        precio_desde: { type: number, format: double }
        moneda: { type: string, default: USD }
        calificacion:
          type: number
          minimum: 0
          maximum: 5
        total_resenas: { type: integer }
        idiomas_disponibles:
          type: array
          items: { type: string }
        disponibilidad:
          $ref: "#/components/schemas/Disponibilidad"
        _links:
          type: object
          additionalProperties:
            type: string
            nullable: true

    AtraccionDetalle:
      allOf:
        - $ref: "#/components/schemas/AtraccionListado"
        - type: object
          properties:
            descripcion: { type: string }
            imagenes:
              type: array
              items: { type: string, format: uri }
            incluye:
              type: array
              items: { type: string }
            no_incluye:
              type: array
              items: { type: string }
            punto_encuentro: { type: string, nullable: true }
            incluye_transporte: { type: boolean }
            incluye_acompaniante: { type: boolean }
            tickets:
              type: array
              items: { $ref: "#/components/schemas/TicketDisponible" }
            horarios_proximos:
              type: array
              items: { $ref: "#/components/schemas/HorarioProximo" }

    OpcionFiltro:
      type: object
      properties:
        name: { type: string }
        tagname: { type: string }
        productCount: { type: integer }
        image:
          nullable: true
          type: object
          properties:
            url: { type: string, format: uri }
        childFilterOptions:
          nullable: true
          type: array
          items:
            $ref: "#/components/schemas/OpcionFiltro"

    FiltrosDisponibles:
      type: object
      properties:
        destinationFilters:
          type: array
          items: { $ref: "#/components/schemas/OpcionFiltro" }
        typeFilters:
          type: array
          items: { $ref: "#/components/schemas/OpcionFiltro" }
        labelFilters:
          type: array
          items: { $ref: "#/components/schemas/OpcionFiltro" }
        minRatingFilter:
          type: array
          items: { $ref: "#/components/schemas/OpcionFiltro" }
        timeOfDayFilters:
          type: array
          items: { $ref: "#/components/schemas/OpcionFiltro" }
        supportedLanguageFilters:
          type: array
          items: { $ref: "#/components/schemas/OpcionFiltro" }
        ufiFilters:
          type: array
          items: { $ref: "#/components/schemas/OpcionFiltro" }

    ClienteInvitadoRequest:
      type: object
      description: Datos del cliente no registrado. Obligatorio cuando la petición no lleva JWT.
      required: [tipo_identificacion, numero_identificacion, correo]
      properties:
        tipo_identificacion: { type: string, maxLength: 20 }
        numero_identificacion: { type: string, maxLength: 20 }
        nombres: { type: string, maxLength: 100, nullable: true }
        apellidos: { type: string, maxLength: 100, nullable: true }
        razon_social: { type: string, maxLength: 200, nullable: true }
        correo: { type: string, format: email, maxLength: 150 }
        telefono: { type: string, maxLength: 20, nullable: true }
        direccion: { type: string, maxLength: 300, nullable: true }

    CrearReservaRequest:
      type: object
      required: [at_guid, hor_guid, lineas]
      properties:
        at_guid:
          type: string
          format: uuid
          description: GUID de la atracción. Debe coincidir con la atracción del horario seleccionado.
        hor_guid: { type: string, format: uuid }
        lineas:
          type: array
          minItems: 1
          items:
            $ref: "#/components/schemas/ReservaLineaRequest"
        origen_canal:
          type: string
          nullable: true
        cliente_invitado:
          description: Requerido si la petición no lleva JWT. Ignorado si el token está presente.
          nullable: true
          allOf:
            - $ref: "#/components/schemas/ClienteInvitadoRequest"

    ReservaLineaRequest:
      type: object
      required: [tck_guid, cantidad]
      properties:
        tck_guid: { type: string, format: uuid }
        cantidad:
          type: integer
          minimum: 1

    ReservaLineaResponse:
      type: object
      properties:
        tck_tipo_participante: { type: string }
        cantidad: { type: integer }
        precio_unit: { type: number, format: double }
        subtotal: { type: number, format: double }

    Reserva:
      type: object
      properties:
        rev_guid: { type: string, format: uuid }
        rev_codigo: { type: string }
        hor_fecha: { type: string, format: date }
        hor_hora_inicio: { type: string }
        hor_hora_fin: { type: string, nullable: true }
        atraccion_nombre: { type: string }
        rev_subtotal: { type: number, format: double }
        rev_valor_iva: { type: number, format: double }
        rev_total: { type: number, format: double }
        moneda: { type: string, default: USD }
        rev_estado: { type: string }
        rev_fecha_reserva_utc: { type: string, format: date-time }
        detalle:
          type: array
          items: { $ref: "#/components/schemas/ReservaLineaResponse" }
        _links:
          type: object
          additionalProperties:
            type: string
            nullable: true

    ConfirmarPagoReservaRequest:
      type: object
      required: [nombre_receptor, correo_receptor]
      properties:
        nombre_receptor: { type: string, maxLength: 100 }
        apellido_receptor: { type: string, maxLength: 100, nullable: true }
        correo_receptor: { type: string, format: email, maxLength: 150 }
        telefono_receptor: { type: string, maxLength: 20, nullable: true }
        observacion: { type: string, maxLength: 500, nullable: true }

    CancelarReservaRequest:
      type: object
      required: [motivo]
      properties:
        motivo: { type: string }

    Factura:
      type: object
      properties:
        fac_guid: { type: string, format: uuid }
        fac_numero: { type: string }
        rev_codigo: { type: string }
        total: { type: number, format: double }
        moneda: { type: string, default: USD }
        fecha_emision: { type: string, format: date-time }
        estado: { type: string, maxLength: 1 }
        nombre_receptor: { type: string }
        correo_receptor: { type: string }

    Paginacion:
      type: object
      properties:
        page: { type: integer }
        limit: { type: integer }
        total: { type: integer }
        total_pages: { type: integer }

    FilterStats:
      type: object
      properties:
        filteredProductCount: { type: integer }
        unfilteredProductCount: { type: integer }

    Sorter:
      type: object
      properties:
        name: { type: string }
        value: { type: string }

    Error:
      type: object
      properties:
        status: { type: integer }
        error: { type: string }
        details:
          type: array
          items: { type: string }
        timestamp:
          type: string
          format: date-time
        path: { type: string }

  responses:
    ListadoAtraccionesOK:
      description: Listado de atracciones obtenido exitosamente
      content:
        application/json:
          schema:
            type: object
            properties:
              status: { type: integer, example: 200 }
              message:
                type: string
                example: Consulta exitosa
              data:
                type: array
                items: { $ref: "#/components/schemas/AtraccionListado" }
              pagination: { $ref: "#/components/schemas/Paginacion" }
              filterStats: { $ref: "#/components/schemas/FilterStats" }
              sorters:
                type: array
                items: { $ref: "#/components/schemas/Sorter" }
              defaultSorter: { $ref: "#/components/schemas/Sorter" }
              _links:
                type: object
                additionalProperties:
                  type: string
                  nullable: true

    DetalleAtraccionOK:
      description: Detalle de atracción obtenido exitosamente
      content:
        application/json:
          schema:
            type: object
            properties:
              status: { type: integer, example: 200 }
              message: { type: string, example: Operación exitosa }
              data: { $ref: "#/components/schemas/AtraccionDetalle" }

    FiltrosOK:
      description: Filtros disponibles obtenidos exitosamente
      content:
        application/json:
          schema:
            type: object
            properties:
              status: { type: integer, example: 200 }
              message: { type: string, example: Operación exitosa }
              data: { $ref: "#/components/schemas/FiltrosDisponibles" }

    ReservaCreada:
      description: Reserva creada exitosamente
      content:
        application/json:
          schema:
            type: object
            properties:
              status: { type: integer, example: 201 }
              message: { type: string, example: Operación exitosa }
              data: { $ref: "#/components/schemas/Reserva" }

    ReservaOK:
      description: Reserva consultada exitosamente
      content:
        application/json:
          schema:
            type: object
            properties:
              status: { type: integer, example: 200 }
              message: { type: string, example: Operación exitosa }
              data: { $ref: "#/components/schemas/Reserva" }

    FacturaOK:
      description: Listado de facturas obtenido exitosamente
      content:
        application/json:
          schema:
            type: object
            properties:
              status: { type: integer, example: 200 }
              data:
                type: array
                items: { $ref: "#/components/schemas/Factura" }
              pagination: { $ref: "#/components/schemas/Paginacion" }

    FacturaItemOK:
      description: Factura generada exitosamente
      content:
        application/json:
          schema:
            type: object
            properties:
              status: { type: integer, example: 201 }
              message: { type: string }
              data: { $ref: "#/components/schemas/Factura" }

    BadRequest:
      description: Error de validación de parámetros o payload
      content:
        application/json:
          schema:
            $ref: "#/components/schemas/Error"

    Unauthorized:
      description: Token ausente o inválido
      content:
        application/json:
          schema:
            $ref: "#/components/schemas/Error"

    Forbidden:
      description: El recurso no pertenece al cliente autenticado
      content:
        application/json:
          schema:
            $ref: "#/components/schemas/Error"

    NotFound:
      description: Recurso no encontrado
      content:
        application/json:
          schema:
            $ref: "#/components/schemas/Error"

    Conflict:
      description: Conflicto de negocio (ej. cupos)
      content:
        application/json:
          schema:
            $ref: "#/components/schemas/Error"

    InternalError:
      description: Error interno del proveedor
      content:
        application/json:
          schema:
            $ref: "#/components/schemas/Error"
```

## 7) Notas de implementación de reservas (referencia operativa)

Para `POST /reservas`, la operación debe ejecutarse en una sola transacción:

1. Crear cabecera `RESERVAS`.
2. Insertar líneas `RESERVA_DETALLE` (una por tipo de ticket).
3. Descontar cupos en `HORARIO` con la suma de cantidades.
4. Validar consistencia monetaria (`subtotal`, `iva`, `total`).

## 8) Compatibilidad hacia adelante

- Campos nuevos de versiones futuras deben ser opcionales.
- Evitar romper nombres de propiedades ya integradas.
- Cualquier cambio breaking requiere nueva versión de contrato.

## 9) Referencia de códigos HTTP por endpoint (implementación API v1)

Convenciones globales (salvo lo indicado):

| Origen | Códigos | Cuerpo de error |
|--------|---------|-----------------|
| `ValidateModelFilter` + reglas de negocio vía `ValidationException` | `400` | `ApiErrorResponse` (snake_case JSON) |
| JWT ausente/inválido (`OnChallenge`) | `401` | Misma forma que `ApiErrorResponse` (snake_case) |
| Política de roles / `ForbiddenBusinessException` | `403` | `ApiErrorResponse` cuando la excepción viene del negocio; el rechazo de **policy** de ASP.NET Core puede usar otro formato |
| `NotFoundException` | `404` | `ApiErrorResponse` |
| `ConflictException` | `409` | `ApiErrorResponse` |
| Excepción no controlada | `500` | `ApiErrorResponse` |

**Login** (`POST /api/v1/admin/auth/login`): se dejan los `ProducesResponseType` y el comportamiento documentado tal como estaban; no forma parte del contrato público de Booking.

| Método y ruta | Éxito | Errores documentados en Swagger (además de 401/403/500 en rutas con `[Authorize]` cuando aplica) |
|---------------|-------|-----------------------------------------------------------------------------------------------------|
| `GET /api/v1/atracciones` | `200` + `ApiListResponse` | `400`, `500` |
| `GET /api/v1/atracciones/filtros` | `200` + `ApiItemResponse` | `400`, `500` |
| `GET /api/v1/atracciones/{guid}` | `200` + `ApiItemResponse` | `404`, `500` |
| `GET /api/v1/atracciones/{guid}/tickets` | `200` | `404`, `500` |
| `GET /api/v1/atracciones/{guid}/horarios-disponibles` | `200` | `404`, `500` |
| `GET /api/v1/tickets/{guid}/horarios` | `200` | `404`, `500` |
| `POST /api/v1/reservas` | `201` + `ApiItemResponse` (`status` 201 en envelope) | `400`, `401`, `404`, `409`, `500` |
| `GET /api/v1/reservas` | `200` + `ApiListResponse` | `401`, `500` |
| `GET /api/v1/reservas/{guid}` | `200` + `ApiItemResponse` | `401`, `403`, `404`, `500` |
| `PUT /api/v1/reservas/{guid}/cancelar` | `204` | `401`, `403`, `404`, `409`, `500` |
| `POST /api/v1/reservas/{guid}/confirmar-pago` | `201` + `ApiItemResponse` | `400`, `404`, `409`, `500` |
| `GET /api/v1/facturas/mis-facturas` | `200` + `ApiListResponse` | `401`, `403`, `500` |
| `GET /api/v1/resenias` | `200` + `ApiItemResponse` | `404`, `500` |
| `POST /api/v1/resenias` | `201` + `ApiItemResponse` (`status` 201) | `400`, `401`, `403`, `404`, `409`, `500` |
| `GET /api/v1/admin/reservas` | `200` + `ApiListResponse` | `400`, `401`, `403`, `500` |
| `GET /api/v1/admin/reservas/{guid}` | `200` + `ApiItemResponse` | `404`, `401`, `403`, `500` |
| `PUT /api/v1/admin/reservas/{guid}/estado` | `204` | `400`, `404`, `409`, `401`, `403`, `500` |
| `GET/POST` facturas, tickets, clientes (admin) | `200` / `201` | `400`, `404`, `409`, `201` |
| `GET/POST/PUT/DELETE` atracciones, destinos, reseñas admin, usuarios | según acción | `400`, `404`, `204`, `401`, `403`, `500` |
| `POST /api/v1/admin/auth/login` | `200` | Sin cambios respecto a la configuración existente del controlador |

---