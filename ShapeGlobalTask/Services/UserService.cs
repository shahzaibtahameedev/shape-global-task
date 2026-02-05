using ShapeGlobalTask.Models;
using ShapeGlobalTask.Repositories;

namespace ShapeGlobalTask.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAIService? _aiService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository, 
        ILogger<UserService> logger,
        IAIService? aiService = null)
    {
        _userRepository = userRepository;
        _logger = logger;
        _aiService = aiService;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        _logger.LogInformation("Retrieving all users");
        
        var users = await _userRepository.GetAllAsync();
        
        _logger.LogInformation("Retrieved {UserCount} users", users.Count());
        return users;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving user by ID: {UserId}", id);
        
        var user = await _userRepository.GetByIdAsync(id);
        
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", id);
        }
        
        return user;
    }

    public async Task<ServiceResult<User>> CreateUserAsync(CreateUserDto createDto, string? correlationId = null)
    {
        _logger.LogInformation(
            "Creating new user with email: {Email}",
            createDto.Email);

        var existingUser = await _userRepository.GetByEmailAsync(createDto.Email);
        
        if (existingUser != null)
        {
            _logger.LogWarning(
                "Cannot create user - email already exists: {Email}",
                createDto.Email);
            
            return ServiceResult<User>.Fail(
                $"A user with email '{createDto.Email}' already exists.",
                "DUPLICATE_EMAIL");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = createDto.FirstName.Trim(),
            LastName = createDto.LastName.Trim(),
            Email = createDto.Email.Trim().ToLowerInvariant(),
            Notes = createDto.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        // If AI service is configured and Notes field has content, analyze it
        if (_aiService != null && !string.IsNullOrWhiteSpace(createDto.Notes))
        {
            _logger.LogInformation(
                "Calling AI service to analyze user notes - CorrelationId: {CorrelationId}",
                correlationId);

            try
            {
                var insights = await _aiService.GenerateInsightsAsync(
                    createDto.Notes, 
                    correlationId);

                if (insights != null)
                {
                    user.SentimentScore = insights.SentimentScore;
                    user.ExtractedTags = insights.Tags;
                    user.EngagementLevel = insights.EngagementLevel;
                    user.LastAnalyzedAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "AI insights applied - Sentiment: {SentimentScore}, Tags: {TagCount}, Engagement: {EngagementLevel}",
                        insights.SentimentScore,
                        insights.Tags.Count,
                        insights.EngagementLevel);
                }
                else
                {
                    _logger.LogWarning("AI service returned no insights - creating user without AI data");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling AI service - creating user without AI data");
            }
        }

        var createdUser = await _userRepository.AddAsync(user);

        _logger.LogInformation(
            "Successfully created user {UserId} with email {Email}",
            createdUser.Id, createdUser.Email);

        return ServiceResult<User>.Ok(createdUser);
    }

    public async Task<ServiceResult<User>> UpdateUserAsync(Guid id, UpdateUserDto updateDto)
    {
        _logger.LogInformation("Updating user: {UserId}", id);

        var existingUser = await _userRepository.GetByIdAsync(id);
        if (existingUser == null)
        {
            _logger.LogWarning("Cannot update - user not found: {UserId}", id);
            return ServiceResult<User>.Fail("User not found.", "NOT_FOUND");
        }

        if (!string.IsNullOrWhiteSpace(updateDto.Email) &&
            !updateDto.Email.Equals(existingUser.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailExists = await _userRepository.GetByEmailAsync(updateDto.Email);
            if (emailExists != null)
            {
                _logger.LogWarning(
                    "Cannot update user {UserId} - email already exists: {Email}",
                    id, updateDto.Email);
                
                return ServiceResult<User>.Fail(
                    $"A user with email '{updateDto.Email}' already exists.",
                    "DUPLICATE_EMAIL");
            }
        }

        if (!string.IsNullOrWhiteSpace(updateDto.FirstName))
            existingUser.FirstName = updateDto.FirstName.Trim();

        if (!string.IsNullOrWhiteSpace(updateDto.LastName))
            existingUser.LastName = updateDto.LastName.Trim();

        if (!string.IsNullOrWhiteSpace(updateDto.Email))
            existingUser.Email = updateDto.Email.Trim().ToLowerInvariant();

        if (updateDto.Notes != null)
            existingUser.Notes = updateDto.Notes.Trim();

        if (updateDto.SentimentScore.HasValue)
        {
            existingUser.SentimentScore = updateDto.SentimentScore.Value;
            existingUser.LastAnalyzedAt = DateTime.UtcNow;
        }

        if (updateDto.ExtractedTags != null)
        {
            existingUser.ExtractedTags = updateDto.ExtractedTags;
            existingUser.LastAnalyzedAt = DateTime.UtcNow;
        }

        if (!string.IsNullOrWhiteSpace(updateDto.EngagementLevel))
        {
            existingUser.EngagementLevel = updateDto.EngagementLevel;
            existingUser.LastAnalyzedAt = DateTime.UtcNow;
        }

        var success = await _userRepository.UpdateAsync(existingUser);
        if (!success)
        {
            _logger.LogError("Failed to update user {UserId} in repository", id);
            return ServiceResult<User>.Fail("Failed to update user.", "UPDATE_FAILED");
        }

        _logger.LogInformation("Successfully updated user {UserId}", id);
        return ServiceResult<User>.Ok(existingUser);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        _logger.LogInformation("Deleting user: {UserId}", id);

        var deleted = await _userRepository.DeleteAsync(id);

        if (deleted)
        {
            _logger.LogInformation("Successfully deleted user {UserId}", id);
        }
        else
        {
            _logger.LogWarning("User not found for deletion: {UserId}", id);
        }

        return deleted;
    }
}
