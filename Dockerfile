# ==============================================================================
# JyanTrack API - Production Multi-Stage Dockerfile (.NET 8 SDK & ASP.NET Core)
# ==============================================================================

# Stage 1: Build & Restore
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file first to leverage Docker layer caching during restore
COPY ["JyanTrackAPI/JyanTrackAPI.csproj", "JyanTrackAPI/"]
RUN dotnet restore "JyanTrackAPI/JyanTrackAPI.csproj"

# Copy source tree and compile release binaries
COPY JyanTrackAPI/ JyanTrackAPI/
WORKDIR "/src/JyanTrackAPI"
RUN dotnet publish "JyanTrackAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Minimal Production Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Configure default container environment
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

# Copy published application with non-root app user ownership
COPY --chown=$APP_UID:$APP_UID --from=build /app/publish .

# Run as non-root user ($APP_UID = 1654 in .NET 8 images) for container hardening
USER $APP_UID

ENTRYPOINT ["dotnet", "JyanTrackAPI.dll"]
