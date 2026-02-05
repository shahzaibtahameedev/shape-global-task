namespace ShapeGlobalTask.Models;

/// <summary>
/// Response from AI service sentiment analysis
/// </summary>
public class SentimentResult
{
    public string Sentiment { get; set; } = string.Empty;
    public double Score { get; set; }
    public double Confidence { get; set; }
}

/// <summary>
/// Response from AI service tag extraction
/// </summary>
public class TagsResult
{
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Response from AI service comprehensive insights
/// </summary>
public class InsightsResult
{
    public string Sentiment { get; set; } = string.Empty;
    public double SentimentScore { get; set; }
    public double Confidence { get; set; }
    public List<string> Tags { get; set; } = new();
    public string EngagementLevel { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Generic API response wrapper from AI service
/// </summary>
/// <typeparam name="T">The data type</typeparam>
public class AIServiceResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public AIServiceError? Error { get; set; }
    public string? CorrelationId { get; set; }
}

/// <summary>
/// Error details from AI service
/// </summary>
public class AIServiceError
{
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
