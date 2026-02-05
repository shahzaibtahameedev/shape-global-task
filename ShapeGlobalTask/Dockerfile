# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ShapeGlobalTask/ShapeGlobalTask.csproj ShapeGlobalTask/
RUN dotnet restore ShapeGlobalTask/ShapeGlobalTask.csproj

# Copy source code
COPY ShapeGlobalTask/ ShapeGlobalTask/

# Build and publish
WORKDIR /src/ShapeGlobalTask
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Copy published application
COPY --from=build /app/publish .

# Create data and logs directories
RUN mkdir -p /app/Data /app/Logs && chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE 5046
EXPOSE 7125

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5046

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
  CMD curl --fail http://localhost:5046/health || exit 1

# Start application
ENTRYPOINT ["dotnet", "ShapeGlobalTask.dll"]
