using Microsoft.AspNetCore.Mvc;
using ShapeGlobalTask.Models;
using ShapeGlobalTask.Services;

namespace ShapeGlobalTask.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        
        _logger.LogInformation(
            "GET /api/users - CorrelationId: {CorrelationId}",
            correlationId);

        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetUserById(Guid id)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        
        _logger.LogInformation(
            "GET /api/users/{UserId} - CorrelationId: {CorrelationId}",
            id, correlationId);

        var user = await _userService.GetUserByIdAsync(id);
        
        if (user == null)
        {
            return NotFound(new { message = $"User with ID '{id}' not found." });
        }

        return Ok(user);
    }

    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserDto createDto)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        
        _logger.LogInformation(
            "POST /api/users - Email: {Email}, CorrelationId: {CorrelationId}",
            createDto.Email, correlationId);

        var result = await _userService.CreateUserAsync(createDto, correlationId);

        if (!result.Success)
        {
            if (result.ErrorCode == "DUPLICATE_EMAIL")
            {
                return Conflict(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        _logger.LogInformation(
            "User created - UserId: {UserId}, CorrelationId: {CorrelationId}",
            result.Data!.Id, correlationId);

        return CreatedAtAction(
            nameof(GetUserById),
            new { id = result.Data!.Id },
            result.Data);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<User>> UpdateUser(Guid id, [FromBody] UpdateUserDto updateDto)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        
        _logger.LogInformation(
            "PUT /api/users/{UserId} - CorrelationId: {CorrelationId}",
            id, correlationId);

        var result = await _userService.UpdateUserAsync(id, updateDto);

        if (!result.Success)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { message = result.ErrorMessage }),
                "DUPLICATE_EMAIL" => Conflict(new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }

        _logger.LogInformation(
            "User updated - UserId: {UserId}, CorrelationId: {CorrelationId}",
            id, correlationId);

        return Ok(result.Data);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        
        _logger.LogInformation(
            "DELETE /api/users/{UserId} - CorrelationId: {CorrelationId}",
            id, correlationId);

        var deleted = await _userService.DeleteUserAsync(id);

        if (!deleted)
        {
            return NotFound(new { message = $"User with ID '{id}' not found." });
        }

        _logger.LogInformation(
            "User deleted - UserId: {UserId}, CorrelationId: {CorrelationId}",
            id, correlationId);

        return NoContent();
    }
}
