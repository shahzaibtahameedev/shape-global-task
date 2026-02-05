using System.ComponentModel.DataAnnotations;

namespace ShapeGlobalTask.Models;

/// <summary>
/// Data Transfer Object for updating an existing user.
/// All fields are optional - only provided fields will be updated.
/// </summary>
public class UpdateUserDto
{
    /// <summary>
    /// User's first name. Max 100 characters.
    /// </summary>
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name. Max 100 characters.
    /// </summary>
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
    public string? LastName { get; set; }

    /// <summary>
    /// User's email address. Must be valid email format if provided.
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
    public string? Email { get; set; }

    /// <summary>
    /// Optional notes about the user. Max 2000 characters.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters.")]
    public string? Notes { get; set; }

    // ==========================================
    // AI-Related Fields (can be updated by AI service)
    // ==========================================

    /// <summary>
    /// AI-computed sentiment score. Range: -1.0 to 1.0.
    /// </summary>
    [Range(-1.0, 1.0, ErrorMessage = "Sentiment score must be between -1.0 and 1.0.")]
    public double? SentimentScore { get; set; }

    /// <summary>
    /// AI-extracted tags/themes.
    /// </summary>
    public List<string>? ExtractedTags { get; set; }

    /// <summary>
    /// AI-determined engagement level.
    /// </summary>
    [RegularExpression("^(Low|Medium|High|VeryHigh)$", ErrorMessage = "Engagement level must be Low, Medium, High, or VeryHigh.")]
    public string? EngagementLevel { get; set; }
}
