# Contexto: raíz del repositorio (Atracciones/)
# docker build -f services/ms-clientes/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY . .
RUN dotnet publish services/ms-clientes/src/Atracciones.MsClientes.Api/Atracciones.MsClientes.Api.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Atracciones.MsClientes.Api.dll"]
