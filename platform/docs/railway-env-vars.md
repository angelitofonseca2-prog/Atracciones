# Variables de entorno Railway

Este archivo documenta las variables de entorno requeridas en cada servicio Railway para que todo funcione en producción.

## gateway (api-gateway)

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
# Destinos YARP → servicios internos Railway (usar private networking)
ReverseProxy__Clusters__identidad__Destinations__d1__Address=http://ms-identidad.railway.internal:8080
ReverseProxy__Clusters__atracciones__Destinations__d1__Address=http://ms-atracciones.railway.internal:8080
ReverseProxy__Clusters__reservas__Destinations__d1__Address=http://ms-reservas.railway.internal:8080
ReverseProxy__Clusters__orquestador__Destinations__d1__Address=http://ms-orquestador.railway.internal:8080
ReverseProxy__Clusters__facturacion__Destinations__d1__Address=http://ms-facturacion.railway.internal:8080
# CORS — dominio del frontend en Railway
Cors__0=https://frontend-atracciones-production.up.railway.app
```

## ms-reservas

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__VentasDb=Host=...;Database=ventas;Username=...;Password=...
ConnectionStrings__CrmDb=Host=...;Database=crm;Username=...;Password=...
# gRPC interno a ms-atracciones (puerto gRPC expuesto)
GrpcClients__Atracciones=http://ms-atracciones.railway.internal:8081
# JWT JWKS igual que en el gateway
Jwt__Authority=https://ms-identidad.railway.internal:8080
# EvBus: dejar en false salvo que RabbitMQ esté desplegado
EvBus__Enabled=false
```

## ms-atracciones

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__InventarioDB=Host=...;...
ConnectionStrings__CatalogosDb=Host=...;...
EvBus__Enabled=false
```

## ms-orquestador

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
GrpcClients__Reservas=http://ms-reservas.railway.internal:8081
GrpcClients__Atracciones=http://ms-atracciones.railway.internal:8081
GrpcClients__Facturacion=http://ms-facturacion.railway.internal:8081
GrpcClients__Identidad=http://ms-identidad.railway.internal:8081
EvBus__Enabled=false
```

## ms-facturacion

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
EvBus__Enabled=false
```

## ms-auditoria

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
EvBus__Enabled=false
```

## ms-identidad

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

## frontend-atracciones

Variables en `railway.json` o Railway dashboard (se pasan como build args en Vite):
```
VITE_API_URL=https://api-gateway-production-0afd.up.railway.app/api/v2
VITE_USE_GRAPHQL=false
```

## Notas importantes

1. **EvBus__Enabled=false** en todos los servicios hasta que RabbitMQ esté desplegado y testado.
2. **Puertos**: Railway expone el servicio en el puerto definido por `ASPNETCORE_URLS` (default 8080); el private networking entre servicios usa ese mismo puerto.
3. **gRPC interno**: los servicios gRPC escuchan en el puerto 8081 (configurado por `KestrelGrpcRestPorts`).
4. Verificar `/health` en cada servicio tras el deploy.
5. **CORS**: en producción poner los orígenes exactos en `Cors__0`, `Cors__1`, etc.
