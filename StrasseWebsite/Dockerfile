# ==========================================
# BASE / RUNTIME
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

USER $APP_UID

WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

# ==========================================
# BUILD
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["StrasseWebsite.csproj", "."]

RUN dotnet restore "./StrasseWebsite.csproj"

COPY . .

RUN dotnet build "./StrasseWebsite.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build \
    --no-restore

# ==========================================
# PUBLISH
# ==========================================
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./StrasseWebsite.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

# ==========================================
# FINAL
# ==========================================
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "StrasseWebsite.dll"]