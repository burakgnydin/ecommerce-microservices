FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/OrderService.Api/OrderService.Api.csproj src/OrderService.Api/
COPY src/OrderService.Application/OrderService.Application.csproj src/OrderService.Application/
COPY src/OrderService.Infrastructure/OrderService.Infrastructure.csproj src/OrderService.Infrastructure/
COPY src/OrderService.Domain/OrderService.Domain.csproj src/OrderService.Domain/
RUN dotnet restore src/OrderService.Api/OrderService.Api.csproj

COPY src/ src/
RUN dotnet publish src/OrderService.Api/OrderService.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "OrderService.Api.dll"]
