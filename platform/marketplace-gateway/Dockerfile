FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY platform/marketplace-gateway ./platform/marketplace-gateway
COPY platform/shared ./platform/shared
RUN dotnet publish platform/marketplace-gateway/Atracciones.MarketplaceGateway.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Atracciones.MarketplaceGateway.dll"]
