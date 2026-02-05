using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShapeGlobalTask.Models;
using ShapeGlobalTask.Repositories;
using Xunit;

namespace ShapeGlobalTask.Tests;

/// <summary>
/// Integration tests for the Users API endpoints.
/// Uses WebApplicationFactory to test the full HTTP pipeline.
/// </summary>
public class UsersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _testDataPath;

    public UsersControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        // Create a unique test data file for each test run
        _testDataPath = Path.Combine(Path.GetTempPath(), $"test_users_{Guid.NewGuid()}.json");

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Override UserDataFilePath for tests
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["UserDataFilePath"] = _testDataPath
                });
            });
        });

        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        // Clean up test data file
        if (File.Exists(_testDataPath))
        {
            File.Delete(_testDataPath);
        }
        return Task.CompletedTask;
    }

    #region GET /api/users Tests

    [Fact]
    public async Task GetAllUsers_ReturnsOkWithEmptyList_WhenNoUsers()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<User>>();
        users.Should().NotBeNull();
    }

    #endregion

    #region POST /api/users Tests

    [Fact]
    public async Task CreateUser_ReturnsCreated_WithValidData()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            FirstName = "Integration",
            LastName = "Test",
            Email = $"integration.test.{Guid.NewGuid()}@example.com",
            Notes = "Created during integration test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdUser = await response.Content.ReadFromJsonAsync<User>();
        createdUser.Should().NotBeNull();
        createdUser!.Id.Should().NotBe(Guid.Empty);
        createdUser.FirstName.Should().Be("Integration");
        createdUser.LastName.Should().Be("Test");
        createdUser.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateUser_ReturnsBadRequest_WithMissingRequiredFields()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            FirstName = "", // Required but empty
            LastName = "Test",
            Email = "test@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_ReturnsBadRequest_WithInvalidEmail()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "not-an-email"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_ReturnsConflict_WithDuplicateEmail()
    {
        // Arrange
        var email = $"duplicate.{Guid.NewGuid()}@example.com";
        var createDto = new CreateUserDto
        {
            FirstName = "First",
            LastName = "User",
            Email = email
        };

        // Create first user
        await _client.PostAsJsonAsync("/api/users", createDto);

        // Try to create another with same email
        createDto.FirstName = "Second";

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion

    #region GET /api/users/{id} Tests

    [Fact]
    public async Task GetUserById_ReturnsOk_WhenUserExists()
    {
        // Arrange - Create a user first
        var createDto = new CreateUserDto
        {
            FirstName = "Find",
            LastName = "Me",
            Email = $"find.me.{Guid.NewGuid()}@example.com"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/users", createDto);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<User>();

        // Act
        var response = await _client.GetAsync($"/api/users/{createdUser!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<User>();
        user.Should().NotBeNull();
        user!.Id.Should().Be(createdUser.Id);
        user.FirstName.Should().Be("Find");
    }

    [Fact]
    public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region PUT /api/users/{id} Tests

    [Fact]
    public async Task UpdateUser_ReturnsOk_WithValidData()
    {
        // Arrange - Create a user first
        var createDto = new CreateUserDto
        {
            FirstName = "Original",
            LastName = "Name",
            Email = $"original.{Guid.NewGuid()}@example.com"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/users", createDto);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<User>();

        var updateDto = new UpdateUserDto
        {
            FirstName = "Updated",
            LastName = "Person"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/users/{createdUser!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedUser = await response.Content.ReadFromJsonAsync<User>();
        updatedUser.Should().NotBeNull();
        updatedUser!.FirstName.Should().Be("Updated");
        updatedUser.LastName.Should().Be("Person");
    }

    [Fact]
    public async Task UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateDto = new UpdateUserDto { FirstName = "Test" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/users/{nonExistentId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/users/{id} Tests

    [Fact]
    public async Task DeleteUser_ReturnsNoContent_WhenUserExists()
    {
        // Arrange - Create a user first
        var createDto = new CreateUserDto
        {
            FirstName = "Delete",
            LastName = "Me",
            Email = $"delete.me.{Guid.NewGuid()}@example.com"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/users", createDto);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<User>();

        // Act
        var response = await _client.DeleteAsync($"/api/users/{createdUser!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify user is gone
        var getResponse = await _client.GetAsync($"/api/users/{createdUser.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Correlation ID Tests

    [Fact]
    public async Task Request_ReturnsCorrelationIdInResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.Headers.Should().ContainKey("X-Correlation-ID");
        var correlationId = response.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
        correlationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Request_UsesProvidedCorrelationId()
    {
        // Arrange
        var customCorrelationId = "test-correlation-123";
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users");
        request.Headers.Add("X-Correlation-ID", customCorrelationId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.Headers.GetValues("X-Correlation-ID").FirstOrDefault().Should().Be(customCorrelationId);
    }

    #endregion
}
