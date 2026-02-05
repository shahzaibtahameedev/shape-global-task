using ShapeGlobalTask.Models;

namespace ShapeGlobalTask.Services;

/// <summary>
/// Interface for AI service communication
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Analyzes the sentiment of the given text
    /// </summary>
    /// <param name="text">Text to analyze</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sentiment analysis result</returns>
    Task<SentimentResult?> AnalyzeSentimentAsync(
        string text, 
        string? correlationId = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts tags/keywords from the given text
    /// </summary>
    /// <param name="text">Text to extract tags from</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted tags</returns>
    Task<TagsResult?> ExtractTagsAsync(
        string text, 
        string? correlationId = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates comprehensive insights from user data
    /// </summary>
    /// <param name="text">User data text to analyze</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive insights</returns>
    Task<InsightsResult?> GenerateInsightsAsync(
        string text, 
        string? correlationId = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the AI service is healthy
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if healthy, false otherwise</returns>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}
