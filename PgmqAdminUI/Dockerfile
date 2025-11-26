# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY PgmqAdminUI.slnx .
COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY global.json .
COPY PgmqAdminUI/PgmqAdminUI.csproj PgmqAdminUI/
COPY PgmqAdminUI.ServiceDefaults/PgmqAdminUI.ServiceDefaults.csproj PgmqAdminUI.ServiceDefaults/

# Restore dependencies
RUN dotnet restore PgmqAdminUI/PgmqAdminUI.csproj

# Copy source code
COPY PgmqAdminUI/ PgmqAdminUI/
COPY PgmqAdminUI.ServiceDefaults/ PgmqAdminUI.ServiceDefaults/

# Build and publish
WORKDIR /src/PgmqAdminUI
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Configure environment
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "PgmqAdminUI.dll"]
