FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7142

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Trippio.Api/Trippio.Api.csproj", "Trippio.Api/"]
COPY ["Trippio.Core/Trippio.Core.csproj", "Trippio.Core/"]
COPY ["Trippio.Data/Trippio.Data.csproj", "Trippio.Data/"]
RUN dotnet restore "Trippio.Api/Trippio.Api.csproj"
COPY . .
WORKDIR "/src/Trippio.Api"
RUN dotnet build "Trippio.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Trippio.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:7142/health || exit 1

ENTRYPOINT ["dotnet", "Trippio.Api.dll"]
