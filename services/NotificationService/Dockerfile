FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/NotificationService.Api/NotificationService.Api.csproj src/NotificationService.Api/
COPY src/NotificationService.Application/NotificationService.Application.csproj src/NotificationService.Application/
COPY src/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj src/NotificationService.Infrastructure/
COPY src/NotificationService.Domain/NotificationService.Domain.csproj src/NotificationService.Domain/
RUN dotnet restore src/NotificationService.Api/NotificationService.Api.csproj

COPY src/ src/
RUN dotnet publish src/NotificationService.Api/NotificationService.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]
