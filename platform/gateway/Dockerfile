# Build desde la raíz del repo: docker build -f platform/gateway/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY platform/gateway ./platform/gateway
COPY platform/shared ./platform/shared
RUN dotnet publish platform/gateway/Atracciones.Gateway.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Atracciones.Gateway.dll"]
