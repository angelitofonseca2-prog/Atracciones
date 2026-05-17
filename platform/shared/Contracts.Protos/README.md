# Contracts.Protos

Contratos **gRPC** compartidos (orquestador ↔ microservicios).

| Archivo | Implementación (proceso dueño) |
|---------|--------------------------------|
| `usuario_service.proto` | **ms-identidad** — `CrearUsuario`, `EliminarUsuario`, … |
| `cliente_service.proto` | **ms-reservas** (CRM fusionado; antes ms-clientes) |
| `reserva_service.proto` | **ms-reservas** — operaciones de venta / reservas |
| `catalogo_service.proto` | **ms-atracciones** (catálogo fusionado; antes ms-catalogos) |
| `atraccion_inventario_service.proto` | **ms-atracciones** — cupos, tickets, … |
| `factura_service.proto` | **ms-facturacion** |
| `auditoria_service.proto` | **ms-auditoria** |

**Orquestador:** `GrpcClients:Clientes` y `GrpcClients:Reservas` deben apuntar al **mismo host** que escucha **ms-reservas** (dos clientes `.proto`, un solo proceso backend).

## Convención prevista

- Un `.proto` por servicio lógico (ej. `usuario_service.proto`, `cliente_service.proto`, …).
- Paquete gRPC por archivo (ej. `atracciones.identidad.v1`).

## Generación de código C#

Cuando existan `.proto`, usar `Grpc.Tools` en un proyecto class library o el script placeholder:

```powershell
.\scripts\generate-protos.ps1
```

La Fase 1+ añadirá el proyecto NuGet local y generación en CI.
