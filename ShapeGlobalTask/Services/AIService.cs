using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ShapeGlobalTask.Models;

namespace ShapeGlobalTask.Services;

/// <summary>
/// HTTP client implementation for AI service communication
/// </summary>
public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AIService(HttpClient httpClient, ILogger<AIService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<SentimentResult?> AnalyzeSentimentAsync(
        string text, 
        string? correlationId = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = CreateRequest("/api/ai/sentiment", text, correlationId);
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "AI sentiment analysis failed with status {StatusCode}",
                    response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<AIServiceResponse<SentimentResult>>(
                _jsonOptions, cancellationToken);

            if (result?.Success != true)
            {
                _logger.LogWarning(
                    "AI sentiment analysis returned error: {Error}",
                    result?.Error?.Message);
                return null;
            }

            return result.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling AI sentiment analysis endpoint");
            return null;
        }
    }

    public async Task<TagsResult?> ExtractTagsAsync(
        string text, 
        string? correlationId = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = CreateRequest("/api/ai/tags", text, correlationId);
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "AI tag extraction failed with status {StatusCode}",
                    response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<AIServiceResponse<TagsResult>>(
                _jsonOptions, cancellationToken);

            if (result?.Success != true)
            {
                _logger.LogWarning(
                    "AI tag extraction returned error: {Error}",
                    result?.Error?.Message);
                return null;
            }

            return result.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling AI tag extraction endpoint");
            return null;
        }
    }

    public async Task<InsightsResult?> GenerateInsightsAsync(
        string text, 
        string? correlationId = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = CreateRequest("/api/ai/insights", text, correlationId);
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "AI insights generation failed with status {StatusCode}",
                    response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<AIServiceResponse<InsightsResult>>(
                _jsonOptions, cancellationToken);

            if (result?.Success != true)
            {
                _logger.LogWarning(
                    "AI insights generation returned error: {Error}",
                    result?.Error?.Message);
                return null;
            }

            return result.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling AI insights endpoint");
            return null;
        }
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI service health check failed");
            return false;
        }
    }

    private HttpRequestMessage CreateRequest(string endpoint, string text, string? correlationId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { text }, _jsonOptions),
                Encoding.UTF8,
                "application/json")
        };

        if (!string.IsNullOrEmpty(correlationId))
        {
            request.Headers.Add("X-Correlation-ID", correlationId);
        }

        return request;
    }
}
