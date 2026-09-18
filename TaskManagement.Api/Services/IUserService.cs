using TaskManagement.Api.DTOs;

namespace TaskManagement.Api.Services;

public interface IUserService
{
    Task<List<UserResponse>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<List<UserResponse>> GetUsersWithoutActiveTasksAsync(CancellationToken cancellationToken = default);
    Task<UserResponse?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserProfileResponse?> GetUserProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserProfileResponse?> UpsertUserProfileAsync(int userId, UpsertUserProfileRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default);
}
