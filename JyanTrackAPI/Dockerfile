# ==============================================================================
# JyanTrack API - Subdirectory Multi-Stage Dockerfile (.NET 8 SDK & ASP.NET Core)
# ==============================================================================

# Stage 1: Build & Restore
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["JyanTrackAPI.csproj", "./"]
RUN dotnet restore "./JyanTrackAPI.csproj"

COPY . .
RUN dotnet publish "JyanTrackAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Minimal Production Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --chown=$APP_UID:$APP_UID --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "JyanTrackAPI.dll"]
