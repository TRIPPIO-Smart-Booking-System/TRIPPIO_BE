FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 7142

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Trippio.Api/Trippio.Api.csproj", "Trippio.Api/"]
COPY ["Trippio.Core/Trippio.Core.csproj", "Trippio.Core/"]
COPY ["Trippio.Data/Trippio.Data.csproj", "Trippio.Data/"]
RUN dotnet restore "Trippio.Api/Trippio.Api.csproj"
COPY . .
WORKDIR "/src/Trippio.Api"
RUN dotnet build "Trippio.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Trippio.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install curl for health check
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER app

ENTRYPOINT ["dotnet", "Trippio.Api.dll"]
