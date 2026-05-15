# Atracciones.Platform.BuildingBlocks

Bloques reutilizables para gateway y futuros microservicios.

| Área | Fase 0 | Próximas fases |
|------|--------|----------------|
| `Middleware/CorrelationIdMiddleware` | Activo en el gateway | Mismo header en servicios ASP.NET Core |
| `Idempotency/IdempotencyKeyExtensions` | No-op (placeholder) | Validación estricta en `ms-orquestador` |
| Cliente gRPC + Polly | — | Fábrica `GrpcChannel` con políticas (timeout, retry, circuit breaker) |
