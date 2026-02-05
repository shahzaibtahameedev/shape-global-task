using ShapeGlobalTask.Models;

namespace ShapeGlobalTask.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid id);

    Task<ServiceResult<User>> CreateUserAsync(CreateUserDto createDto, string? correlationId = null);

    Task<ServiceResult<User>> UpdateUserAsync(Guid id, UpdateUserDto updateDto);

    Task<bool> DeleteUserAsync(Guid id);
}

public class ServiceResult<T>
{
    public bool Success { get; private set; }
    public T? Data { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string? ErrorCode { get; private set; }

    private ServiceResult() { }

    public static ServiceResult<T> Ok(T data) => new()
    {
        Success = true,
        Data = data
    };
    public static ServiceResult<T> Fail(string errorMessage, string? errorCode = null) => new()
    {
        Success = false,
        ErrorMessage = errorMessage,
        ErrorCode = errorCode
    };
}
