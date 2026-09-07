# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore (layer caching)
COPY src/BookStore.Api/BookStore.Api.csproj src/BookStore.Api/
RUN dotnet restore src/BookStore.Api/BookStore.Api.csproj

# Copy everything else and publish
COPY src/ src/
RUN dotnet publish src/BookStore.Api/BookStore.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# curl is used only by the container healthcheck.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && adduser --disabled-password --gecos "" appuser

COPY --from=build /app/publish .

# Persist SQLite outside the container writable layer.
RUN mkdir -p /app/data && chown -R appuser:appuser /app/data

USER appuser

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/bookstore.db"

ENTRYPOINT ["dotnet", "BookStore.Api.dll"]
