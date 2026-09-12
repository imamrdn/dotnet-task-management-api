using TaskManagement.Api.DTOs;

namespace TaskManagement.Api.Services;

public interface IUserService
{
    Task<List<UserResponse>> GetUsersAsync();
    Task<UserResponse?> GetUserByIdAsync(int id);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(int id);
}
