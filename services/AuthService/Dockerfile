FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/AuthService.Api/AuthService.Api.csproj src/AuthService.Api/
COPY src/AuthService.Application/AuthService.Application.csproj src/AuthService.Application/
COPY src/AuthService.Infrastructure/AuthService.Infrastructure.csproj src/AuthService.Infrastructure/
COPY src/AuthService.Domain/AuthService.Domain.csproj src/AuthService.Domain/
RUN dotnet restore src/AuthService.Api/AuthService.Api.csproj

COPY src/ src/
RUN dotnet publish src/AuthService.Api/AuthService.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "AuthService.Api.dll"]
