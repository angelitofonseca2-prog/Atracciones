# Contexto: raíz del repositorio (Atracciones/)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY . .
RUN dotnet publish services/ms-orquestador/src/Atracciones.MsOrquestador.Api/Atracciones.MsOrquestador.Api.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Atracciones.MsOrquestador.Api.dll"]
