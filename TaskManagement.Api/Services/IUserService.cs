using TaskManagement.Api.DTOs;

namespace TaskManagement.Api.Services;

public interface IUserService
{
    Task<List<UserResponse>> GetUsersAsync();
    Task<List<UserResponse>> GetUsersWithoutActiveTasksAsync();
    Task<UserResponse?> GetUserByIdAsync(int id);
    Task<UserProfileResponse?> GetUserProfileAsync(int userId);
    Task<UserProfileResponse?> UpsertUserProfileAsync(int userId, UpsertUserProfileRequest request);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(int id);
}
