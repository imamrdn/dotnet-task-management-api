namespace TaskManagement.Api.DTOs;

public record ApiResponse<T>(bool Success, string Message, T? Data)
{
    public static ApiResponse<T> Ok(string message, T data) => new(true, message, data);

    public static ApiResponse<T> Error(string message) => new(false, message, default);
}
