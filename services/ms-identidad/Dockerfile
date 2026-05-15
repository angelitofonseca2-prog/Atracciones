# Contexto: raíz del repositorio (Atracciones/)
# docker build -f services/ms-identidad/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY . .
RUN dotnet publish services/ms-identidad/src/Atracciones.MsIdentidad.Api/Atracciones.MsIdentidad.Api.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Atracciones.MsIdentidad.Api.dll"]
