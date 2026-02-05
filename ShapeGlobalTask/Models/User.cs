namespace ShapeGlobalTask.Models;

/// <summary>
/// Domain model representing a user in the system.
/// Contains core user information and AI-related fields for future analytics.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Additional notes about the user.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Timestamp when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    // ==========================================
    // AI-Related Fields (populated by AI service)
    // ==========================================

    /// <summary>
    /// AI-computed sentiment score based on user's notes and interactions.
    /// Range: -1.0 (very negative) to 1.0 (very positive).
    /// Null if not yet analyzed.
    /// </summary>
    public double? SentimentScore { get; set; }

    /// <summary>
    /// AI-extracted tags/themes from user's notes and interactions.
    /// Example: ["technology", "finance", "urgent"]
    /// Null if not yet analyzed.
    /// </summary>
    public List<string>? ExtractedTags { get; set; }

    /// <summary>
    /// Timestamp of the last AI analysis performed on this user.
    /// Null if never analyzed.
    /// </summary>
    public DateTime? LastAnalyzedAt { get; set; }

    /// <summary>
    /// AI-determined engagement level of the user.
    /// Possible values: "Low", "Medium", "High", "VeryHigh"
    /// Null if not yet analyzed.
    /// </summary>
    public string? EngagementLevel { get; set; }
}
