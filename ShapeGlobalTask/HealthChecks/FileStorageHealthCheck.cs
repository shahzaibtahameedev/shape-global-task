using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ShapeGlobalTask.HealthChecks;

/// <summary>
/// Health check that verifies the JSON file storage is accessible and writable.
/// </summary>
public class FileStorageHealthCheck : IHealthCheck
{
    private readonly string _filePath;
    private readonly ILogger<FileStorageHealthCheck> _logger;

    public FileStorageHealthCheck(IConfiguration configuration, ILogger<FileStorageHealthCheck> logger)
    {
        _filePath = configuration["UserDataFilePath"]
            ?? Path.Combine(AppContext.BaseDirectory, "Data", "users.json");
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Data directory does not exist: {directory}"));
            }

            if (!File.Exists(_filePath))
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"User data file does not exist yet: {_filePath}. It will be created on first use."));
            }

            using (var stream = File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
            }

            var fileInfo = new FileInfo(_filePath);
            if (fileInfo.IsReadOnly)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"User data file is read-only: {_filePath}"));
            }

            _logger.LogDebug("File storage health check passed for {FilePath}", _filePath);

            return Task.FromResult(HealthCheckResult.Healthy(
                $"File storage is accessible: {_filePath}"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "File storage health check failed - access denied");
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Access denied to file storage: {ex.Message}"));
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "File storage health check failed - IO error");
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"IO error accessing file storage: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File storage health check failed - unexpected error");
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Unexpected error: {ex.Message}"));
        }
    }
}
