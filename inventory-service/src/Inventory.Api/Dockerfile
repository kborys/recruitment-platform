FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["inventory-service/src/Inventory.Api/Inventory.Api.csproj", "inventory-service/src/Inventory.Api/"]
COPY ["inventory-service/src/Inventory.Domain/Inventory.Domain.csproj", "inventory-service/src/Inventory.Domain/"]
COPY ["inventory-service/src/Inventory.Application/Inventory.Application.csproj", "inventory-service/src/Inventory.Application/"]
COPY ["inventory-service/src/Inventory.Infrastructure/Inventory.Infrastructure.csproj", "inventory-service/src/Inventory.Infrastructure/"]
COPY ["contracts/Contracts.csproj", "contracts/"]

RUN dotnet restore "inventory-service/src/Inventory.Api/Inventory.Api.csproj"

COPY ["contracts/", "contracts/"]
COPY ["inventory-service/", "inventory-service/"]

WORKDIR "/src/inventory-service/src/Inventory.Api"
RUN dotnet publish "Inventory.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Inventory.Api.dll"]
