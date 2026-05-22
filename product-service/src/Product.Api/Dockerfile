FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["product-service/src/Product.Api/Product.Api.csproj", "product-service/src/Product.Api/"]
COPY ["product-service/src/Product.Domain/Product.Domain.csproj", "product-service/src/Product.Domain/"]
COPY ["product-service/src/Product.Application/Product.Application.csproj", "product-service/src/Product.Application/"]
COPY ["product-service/src/Product.Infrastructure/Product.Infrastructure.csproj", "product-service/src/Product.Infrastructure/"]
COPY ["contracts/Contracts.csproj", "contracts/"]

RUN dotnet restore "product-service/src/Product.Api/Product.Api.csproj"

COPY ["contracts/", "contracts/"]
COPY ["product-service/", "product-service/"]

WORKDIR "/src/product-service/src/Product.Api"
RUN dotnet publish "Product.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Product.Api.dll"]
