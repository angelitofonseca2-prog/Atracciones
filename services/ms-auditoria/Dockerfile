# Contexto: raíz del repositorio (Atracciones/)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY . .
RUN dotnet publish services/ms-auditoria/src/Atracciones.MsAuditoria.Api/Atracciones.MsAuditoria.Api.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080 8081
ENTRYPOINT ["dotnet", "Atracciones.MsAuditoria.Api.dll"]
