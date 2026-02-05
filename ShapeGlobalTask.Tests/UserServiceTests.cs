using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ShapeGlobalTask.Models;
using ShapeGlobalTask.Repositories;
using ShapeGlobalTask.Services;
using Xunit;

namespace ShapeGlobalTask.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _userService = new UserService(_mockRepository.Object, _mockLogger.Object);
    }

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        // Arrange
        var expectedUsers = new List<User>
        {
            new() { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john@example.com" },
            new() { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
        };
        
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedUsers);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllUsersAsync_WhenNoUsers_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new User 
        { 
            Id = userId, 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "john@example.com" 
        };
        
        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserNotFound_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateUserAsync Tests

    [Fact]
    public async Task CreateUserAsync_WithValidData_CreatesUser()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Notes = "Test notes"
        };

        _mockRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        var result = await _userService.CreateUserAsync(createDto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FirstName.Should().Be("John");
        result.Data.LastName.Should().Be("Doe");
        result.Data.Email.Should().Be("john@example.com");
        
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithDuplicateEmail_ReturnsError()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "existing@example.com"
        };

        var existingUser = new User 
        { 
            Id = Guid.NewGuid(), 
            Email = "existing@example.com" 
        };
        
        _mockRepository.Setup(r => r.GetByEmailAsync("existing@example.com"))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _userService.CreateUserAsync(createDto);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("DUPLICATE_EMAIL");
        result.ErrorMessage.Should().Contain("existing@example.com");
        
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_TrimsWhitespace()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            FirstName = "  John  ",
            LastName = "  Doe  ",
            Email = "  JOHN@EXAMPLE.COM  ",
            Notes = "  Test notes  "
        };

        User? capturedUser = null;
        _mockRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((User u) => u);

        // Act
        var result = await _userService.CreateUserAsync(createDto);

        // Assert
        result.Success.Should().BeTrue();
        capturedUser.Should().NotBeNull();
        capturedUser!.FirstName.Should().Be("John");
        capturedUser.LastName.Should().Be("Doe");
        capturedUser.Email.Should().Be("john@example.com"); // Lowercase and trimmed
        capturedUser.Notes.Should().Be("Test notes");
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    public async Task UpdateUserAsync_WithValidData_UpdatesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var updateDto = new UpdateUserDto
        {
            FirstName = "Johnny",
            LastName = "Updated"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updateDto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.FirstName.Should().Be("Johnny");
        result.Data.LastName.Should().Be("Updated");
        result.Data.Email.Should().Be("john@example.com"); // Unchanged
        
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenUserNotFound_ReturnsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserDto { FirstName = "Johnny" };

        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updateDto);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
        
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_WithDuplicateEmail_ReturnsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            Email = "john@example.com"
        };

        var anotherUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "taken@example.com"
        };

        var updateDto = new UpdateUserDto { Email = "taken@example.com" };

        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        
        _mockRepository.Setup(r => r.GetByEmailAsync("taken@example.com"))
            .ReturnsAsync(anotherUser);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updateDto);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("DUPLICATE_EMAIL");
    }

    [Fact]
    public async Task UpdateUserAsync_WithAIFields_UpdatesAndSetsLastAnalyzedAt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            FirstName = "John",
            Email = "john@example.com"
        };

        var updateDto = new UpdateUserDto
        {
            SentimentScore = 0.85,
            ExtractedTags = new List<string> { "technology", "innovation" },
            EngagementLevel = "High"
        };

        User? capturedUser = null;
        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync(true);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updateDto);

        // Assert
        result.Success.Should().BeTrue();
        capturedUser.Should().NotBeNull();
        capturedUser!.SentimentScore.Should().Be(0.85);
        capturedUser.ExtractedTags.Should().Contain("technology");
        capturedUser.EngagementLevel.Should().Be("High");
        capturedUser.LastAnalyzedAt.Should().NotBeNull();
        capturedUser.LastAnalyzedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region DeleteUserAsync Tests

    [Fact]
    public async Task DeleteUserAsync_WhenUserExists_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(userId), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserNotFound_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
