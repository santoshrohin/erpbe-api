# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first (layer caching)
COPY ErpBE.sln .
COPY ErpBE.API/ErpBE.API.csproj         ErpBE.API/
COPY ErpBE.Application/ErpBE.Application.csproj ErpBE.Application/
COPY ErpBE.Domain/ErpBE.Domain.csproj   ErpBE.Domain/
COPY ErpBE.Infrastructure/ErpBE.Infrastructure.csproj ErpBE.Infrastructure/

RUN dotnet restore ErpBE.API/ErpBE.API.csproj

# Copy source
COPY ErpBE.API/         ErpBE.API/
COPY ErpBE.Application/ ErpBE.Application/
COPY ErpBE.Domain/      ErpBE.Domain/
COPY ErpBE.Infrastructure/ ErpBE.Infrastructure/

RUN dotnet publish ErpBE.API/ErpBE.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=build /app/publish .

# Health check — Swagger ping confirms full startup
HEALTHCHECK --interval=10s --timeout=5s --retries=12 \
  CMD curl -f http://localhost:80/swagger/v1/swagger.json || exit 1

EXPOSE 80
ENTRYPOINT ["dotnet", "ErpBE.API.dll"]
