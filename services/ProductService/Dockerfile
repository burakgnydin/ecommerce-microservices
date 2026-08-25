FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ProductService.Api/ProductService.Api.csproj src/ProductService.Api/
COPY src/ProductService.Application/ProductService.Application.csproj src/ProductService.Application/
COPY src/ProductService.Infrastructure/ProductService.Infrastructure.csproj src/ProductService.Infrastructure/
COPY src/ProductService.Domain/ProductService.Domain.csproj src/ProductService.Domain/
RUN dotnet restore src/ProductService.Api/ProductService.Api.csproj

COPY src/ src/
RUN dotnet publish src/ProductService.Api/ProductService.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "ProductService.Api.dll"]
