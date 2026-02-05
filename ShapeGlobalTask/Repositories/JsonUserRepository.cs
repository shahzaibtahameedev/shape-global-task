using System.Text.Json;
using ShapeGlobalTask.Models;

namespace ShapeGlobalTask.Repositories;

public class JsonUserRepository : IUserRepository
{
    private readonly string _filePath;
    private readonly ILogger<JsonUserRepository> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private List<User> _users = new();
    private bool _isInitialized;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonUserRepository(IConfiguration configuration, ILogger<JsonUserRepository> logger)
    {
        _filePath = configuration["UserDataFilePath"] 
            ?? Path.Combine(AppContext.BaseDirectory, "Data", "users.json");
        _logger = logger;
        
        _logger.LogInformation("JsonUserRepository initialized with file path: {FilePath}", _filePath);
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        await _semaphore.WaitAsync();
        try
        {
            if (_isInitialized) return;

            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogInformation("Created data directory: {Directory}", directory);
            }

            if (File.Exists(_filePath))
            {
                var json = await File.ReadAllTextAsync(_filePath);
                _users = JsonSerializer.Deserialize<List<User>>(json, _jsonOptions) ?? new List<User>();
                _logger.LogInformation("Loaded {UserCount} users from disk", _users.Count);
            }
            else
            {
                _users = new List<User>();
                await PersistToDiskAsync();
                _logger.LogInformation("Created new empty users file at {FilePath}", _filePath);
            }

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize user repository from {FilePath}", _filePath);
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        await EnsureInitializedAsync();
        
        await _semaphore.WaitAsync();
        try
        {
            // Return a copy to prevent external modification
            return _users.Select(u => CloneUser(u)).ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        await EnsureInitializedAsync();
        
        await _semaphore.WaitAsync();
        try
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            return user != null ? CloneUser(user) : null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await EnsureInitializedAsync();
        
        await _semaphore.WaitAsync();
        try
        {
            var user = _users.FirstOrDefault(u => 
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            return user != null ? CloneUser(user) : null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<User> AddAsync(User user)
    {
        await EnsureInitializedAsync();
        
        await _semaphore.WaitAsync();
        try
        {
            var newUser = CloneUser(user);
            if (newUser.Id == Guid.Empty)
            {
                newUser.Id = Guid.NewGuid();
            }
            newUser.CreatedAt = DateTime.UtcNow;

            _users.Add(newUser);
            await PersistToDiskAsync();

            _logger.LogInformation(
                "Added new user {UserId} with email {Email}",
                newUser.Id, newUser.Email);

            return CloneUser(newUser);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> UpdateAsync(User user)
    {
        await EnsureInitializedAsync();
        
        await _semaphore.WaitAsync();
        try
        {
            var existingIndex = _users.FindIndex(u => u.Id == user.Id);
            if (existingIndex == -1)
            {
                _logger.LogWarning("Attempted to update non-existent user {UserId}", user.Id);
                return false;
            }

            // Preserve creation time, update everything else
            var updatedUser = CloneUser(user);
            updatedUser.CreatedAt = _users[existingIndex].CreatedAt;
            
            _users[existingIndex] = updatedUser;
            await PersistToDiskAsync();

            _logger.LogInformation("Updated user {UserId}", user.Id);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await EnsureInitializedAsync();
        
        await _semaphore.WaitAsync();
        try
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                _logger.LogWarning("Attempted to delete non-existent user {UserId}", id);
                return false;
            }

            _users.Remove(user);
            await PersistToDiskAsync();

            _logger.LogInformation("Deleted user {UserId}", id);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        await EnsureInitializedAsync();
        
        await _semaphore.WaitAsync();
        try
        {
            return _users.Any(u => u.Id == id);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task PersistToDiskAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_users, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
            _logger.LogDebug("Persisted {UserCount} users to disk", _users.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist users to {FilePath}", _filePath);
            throw;
        }
    }

    private async Task EnsureInitializedAsync()
    {
        if (!_isInitialized)
        {
            await InitializeAsync();
        }
    }

    private static User CloneUser(User user)
    {
        return new User
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Notes = user.Notes,
            CreatedAt = user.CreatedAt,
            SentimentScore = user.SentimentScore,
            ExtractedTags = user.ExtractedTags?.ToList(),
            LastAnalyzedAt = user.LastAnalyzedAt,
            EngagementLevel = user.EngagementLevel
        };
    }
}
