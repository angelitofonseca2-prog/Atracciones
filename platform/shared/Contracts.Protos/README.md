# Contracts.Protos

Contratos **gRPC** compartidos (orquestador ↔ microservicios).

| Archivo | Servicio |
|---------|----------|
| `usuario_service.proto` | **ms-identidad** — `CrearUsuario`, `EliminarUsuario`, `ObtenerUsuarioPorGuid` |
| `cliente_service.proto` | **ms-clientes** — `CrearCliente`, `EliminarCliente`, `ObtenerClientePorGuid`, `ActualizarCliente` |
| `catalogo_service.proto` | **ms-catalogos** — `GetCatalogosPorGuids` |
| `atraccion_inventario_service.proto` | **ms-atracciones** — `ValidarYReservarCupo`, `LiberarCupo` |

## Convención prevista

- Un `.proto` por servicio (ej. `usuario_service.proto`, `cliente_service.proto`, …).
- Paquete gRPC por archivo (ej. `atracciones.identidad.v1`).

## Generación de código C#

Cuando existan `.proto`, usar `Grpc.Tools` en un proyecto class library o el script placeholder:

```powershell
.\scripts\generate-protos.ps1
```

La Fase 1+ añadirá el proyecto NuGet local y generación en CI.
