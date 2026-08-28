FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/PaymentService.Api/PaymentService.Api.csproj src/PaymentService.Api/
COPY src/PaymentService.Application/PaymentService.Application.csproj src/PaymentService.Application/
COPY src/PaymentService.Infrastructure/PaymentService.Infrastructure.csproj src/PaymentService.Infrastructure/
COPY src/PaymentService.Domain/PaymentService.Domain.csproj src/PaymentService.Domain/
RUN dotnet restore src/PaymentService.Api/PaymentService.Api.csproj

COPY src/ src/
RUN dotnet publish src/PaymentService.Api/PaymentService.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "PaymentService.Api.dll"]
